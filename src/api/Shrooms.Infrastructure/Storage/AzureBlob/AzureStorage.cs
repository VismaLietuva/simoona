using System;
using System.IO;
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

        public async Task<Stream> GetPictureAsync(string blobKey, string tenantPicturesContainer)
        {
            var blobClient = GetBlobClient(blobKey, tenantPicturesContainer);

            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            return await blobClient.OpenReadAsync();
        }

        private BlobClient GetBlobClient(string blobKey, string containerName)
        {
            var blobServiceClient = new BlobServiceClient(_settings.StorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            return containerClient.GetBlobClient(blobKey);
        }
    }
}