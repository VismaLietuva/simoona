using System;
using System.Drawing;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Infrastructure.Storage.AzureBlob
{
    public class AzureStorage : IStorage
    {
        private readonly IApplicationSettings _settings;

        public AzureStorage(IApplicationSettings settings)
        {
            _settings = settings;
        }

        public async Task RemovePictureAsync(string blobKey, string tenantPicturesContainer)
        {
            var blobClient = GetBlobClient(blobKey, tenantPicturesContainer);

            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteAsync();
            }
        }

        [SupportedOSPlatform("windows")]
        public async Task UploadPictureAsync(Image image, string blobKey, string mimeType, string tenantPicturesContainer)
        {
            var blobClient = GetBlobClient(blobKey, tenantPicturesContainer);

            using (var stream = new MemoryStream())
            {
                image.Save(stream, image.RawFormat);
                stream.Position = 0;
                
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = mimeType
                    }
                };
                
                await blobClient.UploadAsync(stream, uploadOptions);
            }
        }

        public async Task UploadPictureAsync(Stream stream, string blobKey, string mimeType, string tenantPicturesContainer)
        {
            var blobClient = GetBlobClient(blobKey, tenantPicturesContainer);
            
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = mimeType
                }
            };
            
            await blobClient.UploadAsync(stream, uploadOptions);
        }

        private BlobClient GetBlobClient(string blobKey, string containerName)
        {
            var blobServiceClient = new BlobServiceClient(_settings.StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            return containerClient.GetBlobClient(blobKey);
        }
    }
}