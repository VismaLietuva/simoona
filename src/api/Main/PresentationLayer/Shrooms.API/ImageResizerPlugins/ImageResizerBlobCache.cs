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
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;

namespace Shrooms.API.ImageResizerPlugins
{
    public class ImageResizerBlobCache : WritableBlobProvider, ICache, IAsyncTyrantCache
    {
        private readonly string _blobStorageConnection;
        private string _blobStorageEndpoint;

        public ImageResizerBlobCache(NameValueCollection args)
        {
            _blobStorageConnection = args["connectionstring"];
            _blobStorageEndpoint = args.GetAsString("blobstorageendpoint", args.GetAsString("endpoint", null));

            VirtualFilesystemPrefix = args["sourcePrefix"];
            BlobContainerName = args["blobContainerName"];
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
            var metaData = FetchMetadata(e.RequestKey, null);

            if (metaData.Exists == true)
            {
                var file = GetFile(e.RequestKey, null);
                var stream = file.Open();
                ((ResponseArgs)e).ResizeImageToStream = s => stream.CopyTo(s);
            }
            else
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
            var metaData = await FetchMetadataAsync(e.RequestCachingKey, null);

            if (metaData.Exists == true)
            {
                var file = await GetFileAsync(e.RequestCachingKey, null);
                var stream = file.Open();
                await e.CreateAndWriteResultAsync(stream, e);
            }
            else
            {
                var stream = new MemoryStream(4096);
                await e.CreateAndWriteResultAsync(stream, e);
                Upload(e.RequestCachingKey, stream, null);
            }

            context.RemapHandler(new NoCacheAsyncHandler(e));
        }
    }
}