using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Shrooms.Infrastructure.Storage.FileSystem
{
    public class FileSystemStorage : IStorage
    {
        private const string StorageFolderName = "storage";

        private readonly IWebHostEnvironment _environment;

        public FileSystemStorage(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public Task RemovePictureAsync(string blobKey, string tenantPicturesContainer)
        {
            var filePath = ResolvePath(blobKey, tenantPicturesContainer);
            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            return Task.CompletedTask;
        }

        public async Task UploadPictureAsync(Stream stream, string blobKey, string mimeType, string tenantPicturesContainer)
        {
            var fullPath = ResolvePath(blobKey, tenantPicturesContainer);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            using var destinationStream = File.Create(fullPath);
            await stream.CopyToAsync(destinationStream);
        }

        public Task<Stream> GetPictureAsync(string blobKey, string tenantPicturesContainer)
        {
            var filePath = ResolvePath(blobKey, tenantPicturesContainer);
            if (!File.Exists(filePath))
            {
                return Task.FromResult<Stream>(null);
            }

            return Task.FromResult<Stream>(File.OpenRead(filePath));
        }

        /// <summary>On-disk path for a blob, guaranteed to stay inside the storage root.</summary>
        private string ResolvePath(string blobKey, string tenantPicturesContainer)
        {
            BlobKeyGuard.EnsureSafeContainer(tenantPicturesContainer);
            BlobKeyGuard.EnsureSafeBlobKey(blobKey);

            var storageRoot = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, StorageFolderName));
            var fullPath = Path.GetFullPath(Path.Combine(storageRoot, tenantPicturesContainer, blobKey));

            var rootWithSeparator = storageRoot.EndsWith(Path.DirectorySeparatorChar)
                ? storageRoot
                : storageRoot + Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(rootWithSeparator, StringComparison.Ordinal))
            {
                throw new ArgumentException("Resolved storage path escapes the storage root.", nameof(blobKey));
            }

            return fullPath;
        }
    }
}
