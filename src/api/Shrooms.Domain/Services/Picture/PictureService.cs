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
            var pictureName = GetNewPictureName(fileName);
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
            if (!await IsRecognizedImageAsync(stream))
            {
                throw new ArgumentException("Image format not recognized.");
            }

            stream.Position = 0;

            var pictureName = GetNewPictureName(fileName);
            var tenantPicturesContainer = await GetPictureContainerAsync(orgId);

            await _storage.UploadPictureAsync(stream, pictureName, mimeType, tenantPicturesContainer);

            return pictureName;
        }

        public async Task RemoveImageAsync(string blobKey, int orgId)
        {
            var tenantPicturesContainer = await GetPictureContainerAsync(orgId);

            await _storage.RemovePictureAsync(blobKey, tenantPicturesContainer);
        }

        private static async Task<bool> IsRecognizedImageAsync(Stream stream)
        {
            var header = new byte[12];
            var read = await stream.ReadAsync(header.AsMemory(0, header.Length));
            if (read < 4)
            {
                return false;
            }

            // JPEG: FF D8 FF
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            {
                return true;
            }

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
            {
                return true;
            }

            // GIF: "GIF8"
            if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
            {
                return true;
            }

            // BMP: "BM"
            if (header[0] == 0x42 && header[1] == 0x4D)
            {
                return true;
            }

            // WebP: "RIFF" ???? "WEBP"
            if (read >= 12
                && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
            {
                return true;
            }

            return false;
        }

        private static string GetNewPictureName(string fileName)
        {
            var id = Guid.NewGuid().ToString();
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

            return $"{id}{extension}";
        }

        private async Task<string> GetPictureContainerAsync(int id)
        {
            var organization = await _organizationsDbSet.FirstAsync(x => x.Id == id);

            return organization.ShortName.ToLowerInvariant();
        }
    }
}
