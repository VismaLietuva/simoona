using Shrooms.Presentation.Common.GeneralCode;
using WebApi.OutputCache.V2;

namespace Shrooms.Presentation.Common.Filters
{
    public class PermissionAwareCacheOutputFilterAttribute : CacheOutputAttribute
    {
        private readonly string[] _permissions;

        public string[] Permissions => _permissions;

        public PermissionAwareCacheOutputFilterAttribute(params string[] permissions)
        {
            CacheKeyGenerator = typeof(PerPermissionCacheKeyGenerator);
            _permissions = permissions;
        }
    }
}
