using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Infrastructure.Storage;

namespace Shrooms.Domain.Services.Picture
{
    public class PictureService : IPictureService
    {
        private readonly IStorage _storage;
        private readonly DbSet<Organization> _organizationsDbSet;

        public PictureService(IStorage storage, IUnitOfWork2 uow)
        {
            _storage = storage;
            _organizationsDbSet = uow.GetDbSet<Organization>();
        }

        public async Task<string> UploadFromStreamAsync(Stream stream, string mimeType, string fileName, int orgId)
        {
            // Legacy endpoint without a magic-byte check: the stored extension comes from the client file
            // name only when it is an allowlisted image extension, otherwise from the (controller-validated)
            // media type. Anything else is rejected so no ".html"/".svg" key can be created.
            var extension = AllowedExtensionFromFileName(fileName) ?? BlobKeyGuard.ExtensionForMimeType(mimeType)
                ?? throw new ArgumentException("Unsupported image type.");

            var pictureName = GetNewPictureName(extension);
            var tenantPicturesContainer = await GetPictureContainerAsync(orgId);

            await _storage.UploadPictureAsync(stream, pictureName, mimeType, tenantPicturesContainer);

            return pictureName;
        }

        public async Task<string> UploadOriginalAsync(Stream stream, string mimeType, string fileName, int orgId)
        {
            // Magic-byte sniff: confirms the upload's leading bytes match a real image
            // format and not a renamed binary. The mime allowlist in the controller is
            // client-asserted and trivially spoofable; this is the server-side check.
            // We intentionally do NOT decode dimensions here — this endpoint streams
            // the bytes to storage verbatim and never decodes them, so the decode-bomb
            // attack surface lives on the serve path, not here.
            var detectedExtension = await DetectImageExtensionAsync(stream);
            if (detectedExtension == null)
            {
                throw new ArgumentException("Image format not recognized.");
            }

            stream.Position = 0;

            // The stored extension follows the detected format, never the client file name, so a
            // "GIF89a<script>" polyglot named x.html is stored (and served) as a .gif.
            var pictureName = GetNewPictureName(detectedExtension);
            var tenantPicturesContainer = await GetPictureContainerAsync(orgId);

            await _storage.UploadPictureAsync(stream, pictureName, mimeType, tenantPicturesContainer);

            return pictureName;
        }

        public async Task RemoveImageAsync(string blobKey, int orgId)
        {
            // Picture ids are stored from client input. A key that is not a bare file name can never have been
            // written by this service, so there is nothing to remove; skipping (rather than throwing) keeps a
            // profile with a tampered picture id editable while the storage layer stays a hard boundary.
            if (!BlobKeyGuard.IsSafeBlobKey(blobKey))
            {
                return;
            }

            var tenantPicturesContainer = await GetPictureContainerAsync(orgId);

            await _storage.RemovePictureAsync(blobKey, tenantPicturesContainer);
        }

        private static async Task<string> DetectImageExtensionAsync(Stream stream)
        {
            var header = new byte[12];
            var read = await stream.ReadAsync(header.AsMemory(0, header.Length));
            if (read < 4)
            {
                return null;
            }

            // JPEG: FF D8 FF
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            {
                return ".jpg";
            }

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
            {
                return ".png";
            }

            // GIF: "GIF8"
            if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
            {
                return ".gif";
            }

            // BMP: "BM"
            if (header[0] == 0x42 && header[1] == 0x4D)
            {
                return ".bmp";
            }

            // WebP: "RIFF" ???? "WEBP"
            if (read >= 12
                && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
            {
                return ".webp";
            }

            return null;
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
