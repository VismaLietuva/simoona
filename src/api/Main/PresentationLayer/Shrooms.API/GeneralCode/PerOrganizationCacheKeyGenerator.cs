using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using Shrooms.API.Helpers;
using WebApi.OutputCache.V2;

namespace Shrooms.API.GeneralCode
{
    public class PerOrganizationCacheKeyGenerator : DefaultCacheKeyGenerator
    {
        public override string MakeCacheKey(HttpActionContext context, MediaTypeHeaderValue mediaType, bool excludeQueryString = false)
        {
            var defaultKey = base.MakeCacheKey(context, mediaType, excludeQueryString);
            var organization = context.Request.GetRequestedTenant();
            var modifiedKey = $"{defaultKey} {organization.GetType().ToString()}={organization};";
            return modifiedKey;
        }
    }
}