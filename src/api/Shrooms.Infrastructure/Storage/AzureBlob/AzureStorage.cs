using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Infrastructure.Storage.AzureBlob
{
    public class AzureStorage : IStorage
    {
        private readonly BlobRequestOptions _blobRequestOptions;

        private readonly IApplicationSettings _settings;

        public AzureStorage(IApplicationSettings settings)
        {
            _settings = settings;

            _blobRequestOptions = new BlobRequestOptions
            {
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(AzureSettings.ExponentialRetryDeltaBackoff), AzureSettings.ExponentialRetryMaxAttempts),
                MaximumExecutionTime = TimeSpan.FromSeconds(AzureSettings.MaximumExecutionTimeInSeconds)
            };
        }

        public async Task RemovePictureAsync(string blobKey, string tenantPicturesContainer)
        {
            var blockBlob = GetBlockBlob(blobKey, tenantPicturesContainer);

            if (await blockBlob.ExistsAsync())
            {
                await blockBlob.DeleteAsync(DeleteSnapshotsOption.None, null, _blobRequestOptions, null);
            }
        }

        public async Task UploadPictureAsync(Image image, string blobKey, string mimeType, string tenantPicturesContainer)
        {
            var blockBlob = GetBlockBlob(blobKey, tenantPicturesContainer);
            blockBlob.Properties.ContentType = mimeType;

            using (var stream = new MemoryStream())
            {
                image.Save(stream, image.RawFormat);
                stream.Position = 0;
                await blockBlob.UploadFromStreamAsync(stream, null, _blobRequestOptions, null);
            }
        }

        public async Task UploadPictureAsync(Stream stream, string blobKey, string mimeType, string tenantPicturesContainer)
        {
            var blockBlob = GetBlockBlob(blobKey, tenantPicturesContainer);
            blockBlob.Properties.ContentType = mimeType;
            await blockBlob.UploadFromStreamAsync(stream, null, _blobRequestOptions, null);
        }

        private CloudBlockBlob GetBlockBlob(string blobKey, string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(_settings.StorageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);

            return container.GetBlockBlobReference(blobKey);
        }
    }
}