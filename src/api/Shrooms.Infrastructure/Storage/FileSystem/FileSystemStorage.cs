using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Shrooms.Infrastructure.Storage.FileSystem
{
    public class FileSystemStorage : IStorage
    {
        private readonly IWebHostEnvironment _environment;

        public FileSystemStorage(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public Task RemovePictureAsync(string blobKey, string tenantPicturesContainer)
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "storage", tenantPicturesContainer, blobKey);
            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            return Task.CompletedTask;
        }

        public async Task UploadPictureAsync(Stream stream, string blobKey, string mimeType, string tenantPicturesContainer)
        {
            var directoryPath = Path.Combine(_environment.ContentRootPath, "storage", tenantPicturesContainer);
            var fullPath = Path.Combine(directoryPath, blobKey);
            Directory.CreateDirectory(directoryPath);

            using var destinationStream = File.Create(fullPath);
            await stream.CopyToAsync(destinationStream);
        }
    }
}