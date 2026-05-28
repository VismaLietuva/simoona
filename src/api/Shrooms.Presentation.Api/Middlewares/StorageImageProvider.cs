using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shrooms.Infrastructure.Storage;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;

namespace Shrooms.Presentation.Api.Middlewares
{
    /// <summary>
    /// ImageSharp.Web image provider that fetches source images via <see cref="IStorage"/>,
    /// so the same code path works for local filesystem in dev and Azure Blob in staging/prod.
    /// Matches paths of the shape "/storage/{tenant}/{file}" (optionally prefixed with "/api/"
    /// for virtual-application hosting) and only processes when resize commands are present
    /// (<see cref="ProcessingBehavior.CommandOnly"/>) — so requests for the original file fall
    /// through to the next middleware (e.g. the redirect-to-blob middleware).
    /// </summary>
    public class StorageImageProvider : IImageProvider
    {
        private static readonly Regex StoragePathRegex = new(
            @"^/?(?:api/)?storage/(?<tenant>[^/]+)/(?<file>.+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly string[] ImageExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

        // IStorage is a scoped, tenant-aware service. ImageSharp.Web registers
        // IImageProvider as a singleton, so we cannot inject IStorage via the
        // constructor — that would capture a single Storage instance and break
        // multi-tenancy. Resolve it per request inside GetAsync instead, via
        // HttpContext.RequestServices (which is the current request's scope).
        public StorageImageProvider()
        {
        }

        /// <summary>
        /// Only kick in when resize commands are on the URL — without commands the
        /// request falls through to <see cref="StorageRedirectMiddleware"/> (302 to blob).
        /// </summary>
        public ProcessingBehavior ProcessingBehavior => ProcessingBehavior.CommandOnly;

        /// <summary>
        /// Whether this provider is interested in the request at all. Cheap path check
        /// before the more expensive <see cref="GetAsync"/>. Allows ImageSharp.Web to
        /// skip our provider entirely for non-storage URLs.
        /// </summary>
        public Func<HttpContext, bool> Match { get; set; } =
            ctx => StoragePathRegex.IsMatch(ctx.Request.Path.Value ?? string.Empty);

        public bool IsValidRequest(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            return StoragePathRegex.IsMatch(path) && HasImageExtension(path);
        }

        public async Task<IImageResolver> GetAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var match = StoragePathRegex.Match(path);
            if (!match.Success)
            {
                return null;
            }

            var tenant = match.Groups["tenant"].Value.ToLowerInvariant();
            var file = match.Groups["file"].Value;

            // Per-request scoped resolution (see constructor comment).
            var storage = context.RequestServices.GetRequiredService<IStorage>();
            var source = await storage.GetPictureAsync(file, tenant);
            if (source == null)
            {
                return null;
            }

            // Buffer the source so ImageSharp.Web can read it (potentially multiple times
            // across processing/caching). Use a recyclable memory stream-style copy to
            // a plain MemoryStream — fine for typical web image sizes (<5 MB upload cap).
            var buffer = new MemoryStream();
            await using (source)
            {
                await source.CopyToAsync(buffer);
            }
            buffer.Position = 0;

            return new StorageImageResolver(buffer);
        }

        private static bool HasImageExtension(string path)
        {
            var ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }

            ext = ext.ToLowerInvariant();
            foreach (var allowed in ImageExtensions)
            {
                if (ext == allowed)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Backing <see cref="IImageResolver"/> that hands ImageSharp.Web the in-memory
    /// buffered source bytes. Uses a sentinel "last modified" so the cache treats the
    /// source as stable — Simoona blobs are GUID-named and effectively immutable, so we
    /// never want the cache to invalidate based on the source timestamp.
    /// </summary>
    public class StorageImageResolver : IImageResolver
    {
        // Use the Unix epoch as a stable sentinel: every cache lookup will see the same
        // LastModified value, so cache entries never appear stale and we always serve
        // from cache after the first hit per (URL + commands) combination.
        private static readonly DateTime StableEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly MemoryStream _content;

        public StorageImageResolver(MemoryStream content)
        {
            _content = content;
        }

        public Task<ImageMetadata> GetMetaDataAsync()
            => Task.FromResult(new ImageMetadata(StableEpoch, _content.Length));

        public Task<Stream> OpenReadAsync()
        {
            // Hand out a fresh read-only stream over the same underlying buffer so
            // ImageSharp.Web can read it without affecting subsequent calls.
            var bytes = _content.ToArray();
            return Task.FromResult<Stream>(new MemoryStream(bytes, writable: false));
        }
    }
}
