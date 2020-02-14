using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using ImageResizer.Storage;

namespace Shrooms.Presentation.Api.ImageResizerPlugins
{
    public abstract class WritableBlobProvider : BlobProviderBase
    {
        protected string BlobContainerName { get; set; }

        protected WritableBlobProvider(NameValueCollection args)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            LoadConfiguration(args);

            LazyExistenceCheck = false;
            ExposeAsVpp = false;
            BlobContainerName = args["blobContainerName"];
        }

        protected string StripPrefixWithTenant(string virtualPath)
        {
            var subPath = StripPrefix(virtualPath).Trim('/', '\\');
            var tenantPartIndex = subPath.IndexOf("/", StringComparison.InvariantCulture);
            subPath = subPath.Substring(tenantPartIndex + 1, subPath.Length - tenantPartIndex - 1);

            return subPath;
        }

        protected string GetTenantPart(string virtualPath)
        {
            var subPath = StripPrefix(virtualPath).Trim('/', '\\');
            var tenantPartIndex = subPath.IndexOf("/", StringComparison.InvariantCulture);
            var tenantPart = subPath.Substring(0, tenantPartIndex);

            return tenantPart;
        }

        protected static string EncodeFileName(string virtualPath)
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

        protected abstract void Upload(string virtualPath, MemoryStream memoryStream, NameValueCollection queryString);

        protected abstract Task UploadAsync(string virtualPath, MemoryStream memoryStream, NameValueCollection queryString);

        protected void ResetBlobMetadataCache(string virtualPath, NameValueCollection queryString)
        {
            if (!CacheMetadata || MetadataCache == null)
            {
                return;
            }

            var key = DeriveMetadataCacheKey(virtualPath, queryString);
            if (MetadataCache.Get(key) is IBlobMetadata)
            {
                HttpRuntime.Cache.Remove(virtualPath);
            }
        }
    }
}