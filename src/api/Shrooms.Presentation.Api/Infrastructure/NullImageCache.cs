using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Resolvers;

namespace Shrooms.Presentation.Api.Caching
{
    public class NullImageCache : IImageCache
    {
        public Task<IImageCacheResolver> GetAsync(string key) =>
            Task.FromResult<IImageCacheResolver>(null);

        public Task SetAsync(string key, Stream stream, ImageCacheMetadata metadata) =>
            Task.CompletedTask;
    }
}
