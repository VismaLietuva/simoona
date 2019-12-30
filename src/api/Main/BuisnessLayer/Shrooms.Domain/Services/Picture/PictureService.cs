using System;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Azure;
using Shrooms.DataLayer.DAL;
using Shrooms.EntityModels.Models;

namespace Shrooms.Domain.Services.Picture
{
    public class PictureService : IPictureService
    {
        private readonly IStorage _storage;
        private readonly IDbSet<Organization> _organizationsDbSet;

        public PictureService(IStorage storage, IUnitOfWork2 uow)
        {
            _storage = storage;
            _organizationsDbSet = uow.GetDbSet<Organization>();
        }

        public async Task<string> UploadFromImage(Image image, string mimeType, string fileName, int orgId)
        {
            var pictureName = GetNewPictureName(fileName);
            var tenantPicturesContainer = await GetPictureContainer(orgId);

            await _storage.UploadPicture(image, pictureName, mimeType, tenantPicturesContainer);

            return pictureName;
        }

        public async Task<string> UploadFromStream(Stream stream, string mimeType, string fileName, int orgId)
        {
            var pictureName = GetNewPictureName(fileName);
            var tenantPicturesContainer = await GetPictureContainer(orgId);

            await _storage.UploadPicture(stream, pictureName, mimeType, tenantPicturesContainer);

            return pictureName;
        }

        public async Task RemoveImage(string blobKey, int orgId)
        {
            var tenantPicturesContainer = await GetPictureContainer(orgId);

            await _storage.RemovePicture(blobKey, tenantPicturesContainer);
        }

        private static string GetNewPictureName(string fileName)
        {
            var id = Guid.NewGuid().ToString();
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return $"{id}{extension}";
        }

        private async Task<string> GetPictureContainer(int id)
        {
            var organization = await _organizationsDbSet.FirstAsync(x => x.Id == id);

            return organization.ShortName.ToLowerInvariant();
        }
    }
}