using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Shrooms.Presentation.Common.Helpers;

namespace Shrooms.Presentation.Common.Filters
{
    public class WidgetCacheInvalidator : IWidgetCacheInvalidator
    {
        private readonly IOutputCacheStore _outputCacheStore;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WidgetCacheInvalidator(IOutputCacheStore outputCacheStore, IHttpContextAccessor httpContextAccessor)
        {
            _outputCacheStore = outputCacheStore;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task InvalidateAsync(string cacheGroup, CancellationToken cancellationToken = default)
        {
            var tenant = _httpContextAccessor.HttpContext?.GetRequestedTenant();

            if (string.IsNullOrEmpty(tenant))
            {
                return;
            }

            await _outputCacheStore.EvictByTagAsync(WidgetCacheTag.For(cacheGroup, tenant), cancellationToken);
        }
    }
}
