using System;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Infrastructure.Storage;

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

        public async Task<string> UploadFromImageAsync(Image image, string mimeType, string fileName, int orgId)
        {
            var pictureName = GetNewPictureName(fileName);
            var tenantPicturesContainer = GetPictureContainer(orgId);

            await _storage.UploadPictureAsync(image, pictureName, mimeType, tenantPicturesContainer);

            return pictureName;
        }

        public async Task<string> UploadFromStreamAsync(Stream stream, string mimeType, string fileName, int orgId)
        {
            var pictureName = GetNewPictureName(fileName);
            var tenantPicturesContainer = GetPictureContainer(orgId);

            await _storage.UploadPictureAsync(stream, pictureName, mimeType, tenantPicturesContainer);

            return pictureName;
        }

        public async Task RemoveImageAsync(string blobKey, int orgId)
        {
            var tenantPicturesContainer = GetPictureContainer(orgId);

            await _storage.RemovePictureAsync(blobKey, tenantPicturesContainer);
        }

        private static string GetNewPictureName(string fileName)
        {
            var id = Guid.NewGuid().ToString();
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

            return $"{id}{extension}";
        }

        private string GetPictureContainer(int id)
        {
            var organization = _organizationsDbSet.First(x => x.Id == id);

            return organization.ShortName.ToLowerInvariant();
        }
    }
}