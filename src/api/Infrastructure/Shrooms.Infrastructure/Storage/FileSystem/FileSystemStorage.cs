using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Web.Hosting;
using Shrooms.Azure;

namespace Shrooms.Infrastructure.Storage.FileSystem
{
    public class FileSystemStorage : IStorage
    {
        public Task RemovePicture(string blobKey, string tenantPicturesContainer)
        {
            var filePath = HostingEnvironment.MapPath("~/storage/" + tenantPicturesContainer + "/" + blobKey);
            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            return Task.FromResult<object>(null);
        }

        public Task UploadPicture(Image image, string blobKey, string mimeType, string tenantPicturesContainer)
        {
            var filePath = HostingEnvironment.MapPath("~/storage/" + tenantPicturesContainer + "/");
            var fullPath = Path.Combine(filePath, blobKey);
            Directory.CreateDirectory(filePath);

            image.Save(fullPath);

            return Task.CompletedTask;
        }

        public async Task UploadPicture(Stream stream, string blobKey, string mimeType, string tenantPicturesContainer)
        {
            var filePath = HostingEnvironment.MapPath("~/storage/" + tenantPicturesContainer + "/");
            var fullPath = Path.Combine(filePath, blobKey);
            Directory.CreateDirectory(filePath);

            var destinationStream = File.Create(fullPath);
            await stream.CopyToAsync(destinationStream);
        }
    }
}