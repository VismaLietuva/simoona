using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using ImageResizer.Storage;
using ImageResizer.Util;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Shrooms.API.ImageResizerPlugins
{
    public class WritableBlobProvider : BlobProviderBase
    {
        protected CloudBlobClient _cloudBlobClient;
        protected string BlobContainerName { get; set; }

        private string StripPrefixWithTenant(string virtualPath)
        {
            var subPath = StripPrefix(virtualPath).Trim('/', '\\');
            var tenantPartIndex = subPath.IndexOf("/", StringComparison.InvariantCulture);
            subPath = subPath.Substring(tenantPartIndex + 1, subPath.Length - tenantPartIndex - 1);

            return subPath;
        }

        private ICloudBlob GetBlobRef(string virtualPath)
        {
            return AsyncUtils.RunSync(() => GetBlobRefAsync(virtualPath));
        }

        private Task<ICloudBlob> GetBlobRefAsync(string virtualPath)
        {
            var subPath = StripPrefixWithTenant(virtualPath);
            var fileName = EncodeFileName(subPath);

            var relativeBlobUrl = $"{_cloudBlobClient.BaseUri.OriginalString.TrimEnd('/', '\\')}/{BlobContainerName}/{fileName}";
            return _cloudBlobClient.GetBlobReferenceFromServerAsync(new Uri(relativeBlobUrl));
        }

        private CloudBlockBlob GetBlockBlobRefAsync(string virtualPath)
        {
            var blobContainer = _cloudBlobClient.GetContainerReference(BlobContainerName);
            blobContainer.CreateIfNotExists();

            var subPath = StripPrefixWithTenant(virtualPath).Trim('/', '\\');
            var fileName = EncodeFileName(subPath);
            return blobContainer.GetBlockBlobReference(fileName);
        }

        private static string EncodeFileName(string virtualPath)
        {
            var queryStringIndex = virtualPath.IndexOf("?", StringComparison.InvariantCulture);

            var fileName = virtualPath.Substring(0, queryStringIndex);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName).ToLowerInvariant();
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            var queryStringValue = virtualPath.Substring(queryStringIndex + 1, virtualPath.Length - queryStringIndex - 1);

            var additionalSeparatorIndex = queryStringValue.IndexOf("|", StringComparison.InvariantCulture);
            if (additionalSeparatorIndex != -1)
            {
                queryStringValue = queryStringValue.Substring(0, additionalSeparatorIndex);
            }

            var queryString = HttpUtility.ParseQueryString(queryStringValue);
            var width = queryString["width"] ?? "0";
            var height = queryString["height"] ?? "0";
            var mode = queryString["mode"] ?? "max";

            var finalName = $"{fileNameWithoutExtension}-{width}-{height}-{mode}{extension}";

            return finalName;
        }

        protected IBlobMetadata FetchMetadata(string virtualPath, NameValueCollection queryString)
        {
            return AsyncUtils.RunSync(() => FetchMetadataAsync(virtualPath, queryString));
        }

        public override async Task<IBlobMetadata> FetchMetadataAsync(string virtualPath, NameValueCollection queryString)
        {
            try
            {
                var cloudBlob = await GetBlobRefAsync(virtualPath);
                var meta = new BlobMetadata { Exists = true };

                var utc = cloudBlob.Properties.LastModified;
                if (utc != null)
                {
                    meta.LastModifiedDateUtc = utc.Value.UtcDateTime;
                }

                return meta;
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 404)
                {
                    return new BlobMetadata { Exists = false };
                }

                throw;
            }
        }

        public override async Task<Stream> OpenAsync(string virtualPath, NameValueCollection queryString)
        {
            var memoryStream = new MemoryStream(4096);

            try
            {
                var cloudBlob = await GetBlobRefAsync(virtualPath);
                await cloudBlob.DownloadToStreamAsync(memoryStream);
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 404)
                {
                    throw new FileNotFoundException("Azure blob file not found", e);
                }

                throw;
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        protected void Upload(string virtualPath, MemoryStream memoryStream, NameValueCollection queryString)
        {
            AsyncUtils.RunSync(() => UploadAsync(virtualPath, memoryStream, queryString));
        }

        protected async Task UploadAsync(string virtualPath, MemoryStream memoryStream, NameValueCollection queryString)
        {
            var cloudBlob = GetBlockBlobRefAsync(virtualPath);

            memoryStream.Seek(0, SeekOrigin.Begin);
            await cloudBlob.UploadFromStreamAsync(memoryStream);
        }
    }
}