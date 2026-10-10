using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Infrastructure.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace Shrooms.Domain.Services.Picture
{
    public class PictureService : IPictureService
    {
        /// <summary>
        /// Largest image accepted for storage, in pixels. Anonymous resize requests decode stored images,
        /// so a 40000x40000 PNG (a few hundred KB on disk, several GB decoded) must never get in.
        /// </summary>
        public const long MaxPixels = 40_000_000;

        /// <summary>Frame budget for animated GIFs; every frame decodes to a full canvas buffer.</summary>
        public const int MaxGifFrames = 200;

        private readonly IStorage _storage;
        private readonly DbSet<Organization> _organizationsDbSet;
        private readonly IPictureReferenceService _pictureReferences;

        public PictureService(IStorage storage, IUnitOfWork2 uow, IPictureReferenceService pictureReferences)
        {
            _storage = storage;
            _organizationsDbSet = uow.GetDbSet<Organization>();
            _pictureReferences = pictureReferences;
        }

        public async Task<string> UploadFromStreamAsync(Stream stream, string mimeType, string fileName, int orgId)
        {
            // Legacy endpoint: the client must at least declare an image (allowlisted file extension or
            // media type), but the stored extension follows the format detected from the bytes, exactly as
            // UploadOriginalAsync does, so a PNG uploaded as "photo.jpg" is stored and served as PNG.
            if (AllowedExtensionFromFileName(fileName) == null && BlobKeyGuard.ExtensionForMimeType(mimeType) == null)
            {
                throw new ArgumentException("Unsupported image type.");
            }

            var (_, format) = ValidateDimensions(stream);
            var extension = BlobKeyGuard.ExtensionForMimeType(format?.DefaultMimeType)
                ?? throw new ArgumentException("Image format not recognized.");

            var pictureName = GetNewPictureName(extension);
            var tenantPicturesContainer = await GetPictureContainerAsync(orgId);

            await _storage.UploadPictureAsync(stream, pictureName, mimeType, tenantPicturesContainer);

            return pictureName;
        }

        public async Task<string> UploadOriginalAsync(Stream stream, string mimeType, string fileName, int orgId)
        {
            // The real format comes from the bytes, never from the client's file name or media type, so a
            // "GIF89a<script>" polyglot named x.html is either a valid GIF stored as .gif or rejected.
            var (_, format) = ValidateDimensions(stream);
            var detectedExtension = BlobKeyGuard.ExtensionForMimeType(format?.DefaultMimeType)
                ?? throw new ArgumentException("Image format not recognized.");

            var pictureName = GetNewPictureName(detectedExtension);
            var tenantPicturesContainer = await GetPictureContainerAsync(orgId);

            await _storage.UploadPictureAsync(stream, pictureName, mimeType, tenantPicturesContainer);

            return pictureName;
        }

        public async Task RemoveImageAsync(string blobKey, int orgId)
        {
            if (!IsRemovableKey(blobKey))
            {
                return;
            }

            // Only a picture that no stored record references any more is deleted. Callers therefore save
            // their own change first and remove the previous picture afterwards. Picture keys are public
            // (every avatar URL shows one), so a user could otherwise set their PictureId to a colleague's
            // avatar and have it deleted on the next change.
            if (await _pictureReferences.CountReferencesAsync(blobKey) > 0)
            {
                return;
            }

            await _storage.RemovePictureAsync(blobKey, await GetPictureContainerAsync(orgId));
        }

        public async Task RemoveImageIgnoringReferencesAsync(string blobKey, int orgId)
        {
            if (!IsRemovableKey(blobKey))
            {
                return;
            }

            await _storage.RemovePictureAsync(blobKey, await GetPictureContainerAsync(orgId));
        }

        public async Task<bool> IsInUseAsync(string blobKey)
        {
            return !string.IsNullOrEmpty(blobKey) && await _pictureReferences.CountReferencesAsync(blobKey) > 0;
        }

        // Picture ids are stored from client input. A key that is not a bare file name can never have been
        // written by this service, so there is nothing to remove; skipping (rather than throwing) keeps a
        // profile with a tampered picture id editable while the storage layer stays a hard boundary.
        private static bool IsRemovableKey(string blobKey)
        {
            return BlobKeyGuard.IsSafeBlobKey(blobKey) && BlobKeyGuard.HasAllowedImageExtension(blobKey);
        }

        /// <summary>
        /// Reads only the image header: rejects anything that is not a decodable image and anything whose
        /// declared canvas exceeds <see cref="MaxPixels"/>. Leaves the stream at position 0.
        /// </summary>
        private static (IImageInfo Info, IImageFormat Format) ValidateDimensions(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentException("No image data.");
            }

            IImageInfo info;
            IImageFormat format;
            try
            {
                info = Image.Identify(stream, out format);
            }
            catch (Exception ex) when (ex is UnknownImageFormatException || ex is InvalidImageContentException || ex is NotSupportedException)
            {
                throw new ArgumentException("Image format not recognized.", ex);
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }
            }

            if (info == null)
            {
                throw new ArgumentException("Image format not recognized.");
            }

            if ((long)info.Width * info.Height > MaxPixels)
            {
                throw new ArgumentException($"Image is too large: at most {MaxPixels / 1_000_000} megapixels are allowed.");
            }

            // Identify sees one frame. An animated GIF decodes to frames x canvas, so the frame count and
            // the total pixel volume are bounded separately.
            if (string.Equals(format?.DefaultMimeType, "image/gif", StringComparison.OrdinalIgnoreCase))
            {
                var frames = GifFrameCounter.Count(stream, MaxGifFrames);
                if (frames > MaxGifFrames || (long)frames * info.Width * info.Height > MaxPixels * 2)
                {
                    throw new ArgumentException($"Animated GIF is too large: at most {MaxGifFrames} frames are allowed.");
                }
            }

            return (info, format);
        }

        private static string AllowedExtensionFromFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return BlobKeyGuard.HasAllowedImageExtension(extension) ? extension : null;
        }

        private static string GetNewPictureName(string extension)
        {
            return $"{Guid.NewGuid()}{extension}";
        }

        private async Task<string> GetPictureContainerAsync(int id)
        {
            var organization = await _organizationsDbSet.FirstAsync(x => x.Id == id);

            return organization.ShortName.ToLowerInvariant();
        }
    }
}
