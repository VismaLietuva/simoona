using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.Common.Helpers;

namespace Shrooms.Presentation.Common.GeneralCode
{
    public class PerPermissionCacheKeyGenerator : PerOrganizationCacheKeyGenerator
    {
        public override string MakeCacheKey(HttpActionContext context, MediaTypeHeaderValue mediaType, bool excludeQueryString = false)
        {
            var baseKey = base.MakeCacheKey(context, mediaType, excludeQueryString);
            var cacheAttribute = context.ActionDescriptor
                .GetCustomAttributes<PermissionAwareCacheOutputFilterAttribute>()
                .FirstOrDefault();
            if (cacheAttribute == null)
            {
                return baseKey;
            }

            var permissions = cacheAttribute.Permissions;
            var dependencyScope = context.Request.GetDependencyScope();
            var permissionService = dependencyScope.GetService(typeof(IPermissionService)) as IPermissionService;
            var userAndOrganization = context.RequestContext.Principal.Identity.GetUserAndOrganization();
            var userPermissions = permissions.Where(p => permissionService != null && permissionService.UserHasPermission(userAndOrganization, p));
            var permissionKey = string.Join(",", userPermissions);
            return $"{baseKey};{permissionKey};";
        }
    }
}
