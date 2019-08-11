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
            var tenantPicturesContainer = _organizationsDbSet.Where(o => o.Id == orgId).Select(o => o.ShortName).First().ToLowerInvariant();

            await _storage.UploadPicture(image, pictureName, mimeType, tenantPicturesContainer);

            return pictureName;
        }

        public async Task<string> UploadFromStream(Stream stream, string mimeType, string fileName, int orgId)
        {
            var pictureName = GetNewPictureName(fileName);
            var tenantPicturesContainer = _organizationsDbSet.Where(o => o.Id == orgId).Select(o => o.ShortName).First().ToLowerInvariant();

            await _storage.UploadPicture(stream, pictureName, mimeType, tenantPicturesContainer);

            return pictureName;
        }

        private static string GetNewPictureName(string fileName)
        {
            var id = Guid.NewGuid().ToString();
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return $"{id}{extension}";
        }
    }
}