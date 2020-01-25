using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using ImageResizer;
using ImageResizer.Caching;
using ImageResizer.Configuration;
using ImageResizer.ExtensionMethods;
using ImageResizer.Plugins;
using ImageResizer.Plugins.Basic;
using ImageResizer.Storage;
using ImageResizer.Util;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Shrooms.API.ImageResizerPlugins
{
    public class ImageResizerBlobCache : WritableBlobProvider, ICache, IAsyncTyrantCache
    {
        private string _blobStorageConnection;
        private string _blobStorageEndpoint;
        private CloudBlobClient _cloudBlobClient;

        public ImageResizerBlobCache(NameValueCollection args)
            : base(args)
        {
            _blobStorageConnection = args["connectionstring"];
            _blobStorageEndpoint = args.GetAsString("blobstorageendpoint", args.GetAsString("endpoint", null));
        }

        public override IPlugin Install(Config config)
        {
            if (string.IsNullOrEmpty(_blobStorageConnection))
            {
                throw new InvalidOperationException("ImageResizerBlobCache requires a named connection string or a connection string to be specified with the 'connectionString' attribute.");
            }

            // Setup the connection to Windows Azure Storage for compatibility, look up the appSetting first.
            var connectionString = CloudConfigurationManager.GetSetting(_blobStorageConnection);
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = ConfigurationManager.ConnectionStrings[_blobStorageConnection]?.ConnectionString;
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _blobStorageConnection;
            }

            if (CloudStorageAccount.TryParse(connectionString, out var cloudStorageAccount))
            {
                if (string.IsNullOrEmpty(_blobStorageEndpoint))
                {
                    _blobStorageEndpoint = cloudStorageAccount.BlobEndpoint.ToString();
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid ImageResizerBlobCache connectionString value; rejected by Azure SDK.");
            }

            if (!_blobStorageEndpoint.EndsWith("/"))
            {
                _blobStorageEndpoint += "/";
            }

            _cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            config.Plugins.add_plugin(this);
            return this;
        }

        public override bool Uninstall(Config config)
        {
            config.Plugins.remove_plugin(this);
            return true;
        }

        public bool CanProcess(HttpContext context, IResponseArgs e)
        {
            if (((ResizeSettings)e.RewrittenQuerystring).Cache == ServerCacheMode.No)
            {
                return false;
            }

            return true;
        }

        public void Process(HttpContext context, IResponseArgs e)
        {
            var file = GetFile(e.RequestKey, null);
            var reprocessCache = false;

            if (file != null)
            {
                try
                {
                    var stream = file.Open();
                    ((ResponseArgs)e).ResizeImageToStream = s => stream.CopyTo(s);
                }
                catch (FileNotFoundException)
                {
                    reprocessCache = true;
                }
            }

            if (file == null || reprocessCache)
            {
                var stream = new MemoryStream(4096);
                e.ResizeImageToStream(stream);
                Upload(e.RequestKey, stream, null);
            }

            context.RemapHandler(new NoCacheHandler(e));
        }

        public bool CanProcess(HttpContext context, IAsyncResponsePlan e)
        {
            //Disk caching will 'pass on' caching requests if 'cache=no'.
            if (new Instructions(e.RewrittenQuerystring).Cache == ServerCacheMode.No)
            {
                return false;
            }

            return true;
        }

        public async Task ProcessAsync(HttpContext context, IAsyncResponsePlan e)
        {
            var file = await GetFileAsync(e.RequestCachingKey, null);
            var reprocessCache = false;

            if (file != null)
            {
                try
                {
                    var stream = await file.OpenAsync();
                    await e.CreateAndWriteResultAsync(stream, e);
                }
                catch (FileNotFoundException)
                {
                    reprocessCache = true;
                }
            }

            if (file == null || reprocessCache)
            {
                var stream = new MemoryStream(4096);
                await e.CreateAndWriteResultAsync(stream, e);
                Upload(e.RequestCachingKey, stream, null);
            }

            context.RemapHandler(new NoCacheAsyncHandler(e));
        }

        private Task<ICloudBlob> GetBlobRefAsync(string virtualPath)
        {
            var subPath = StripPrefixWithTenant(virtualPath);
            var fileName = EncodeFileName(subPath);
            var tenantPart = GetTenantPart(virtualPath);

            var relativeBlobUrl = $"{_cloudBlobClient.BaseUri.OriginalString.TrimEnd('/', '\\')}/{BlobContainerName}/{tenantPart}/{fileName}";
            return _cloudBlobClient.GetBlobReferenceFromServerAsync(new Uri(relativeBlobUrl));
        }

        private CloudBlockBlob GetBlockBlobRef(string virtualPath)
        {
            var blobContainer = _cloudBlobClient.GetContainerReference(BlobContainerName);
            blobContainer.CreateIfNotExists();

            var subPath = StripPrefixWithTenant(virtualPath).Trim('/', '\\');
            var fileName = EncodeFileName(subPath);
            var tenantPart = GetTenantPart(virtualPath);

            var fullFilePath = $"{tenantPart}/{fileName}";
            return blobContainer.GetBlockBlobReference(fullFilePath);
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

        protected override void Upload(string virtualPath, MemoryStream memoryStream, NameValueCollection queryString)
        {
            AsyncUtils.RunSync(() => UploadAsync(virtualPath, memoryStream, queryString));
        }

        protected override async Task UploadAsync(string virtualPath, MemoryStream memoryStream, NameValueCollection queryString)
        {
            ResetBlobMetadataCache(virtualPath, queryString);

            var cloudBlob = GetBlockBlobRef(virtualPath);

            memoryStream.Seek(0, SeekOrigin.Begin);
            await cloudBlob.UploadFromStreamAsync(memoryStream);
        }
    }
}