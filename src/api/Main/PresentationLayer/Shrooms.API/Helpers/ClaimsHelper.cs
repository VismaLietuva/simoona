using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Shrooms.Constants.DataLayer;
using Shrooms.Constants.WebApi;

namespace Shrooms.API.Helpers
{
    public static class ClaimsHelper
    {
        public static int GetOrganizationId(this IIdentity identity)
        {
            if (identity == null)
            {
                return 0;
            }

            var claimsIdentity = identity as ClaimsIdentity;
            var orgIdClaim = claimsIdentity.FindFirst(WebApiConstants.ClaimOrganizationId);
            if (orgIdClaim == null)
            {
                return 0;
            }

            return int.Parse(orgIdClaim.Value);
        }

        public static string GetOrganizationName(this IIdentity identity)
        {
            if (identity == null)
            {
                return string.Empty;
            }

            var claimsIdentity = identity as ClaimsIdentity;
            var orgNameClaim = claimsIdentity.FindFirst(WebApiConstants.ClaimOrganizationName);
            if (orgNameClaim == null)
            {
                return string.Empty;
            }

            return orgNameClaim.Value;
        }

        public static string GetOriginalUsername(this IPrincipal principal)
        {
            if (principal == null)
            {
                return string.Empty;
            }

            var claimsPrincipal = principal as ClaimsPrincipal;
            if (claimsPrincipal == null)
            {
                return string.Empty;
            }

            if (!claimsPrincipal.HasClaim(ConstDataLayer.ClaimUserImpersonation, true.ToString()))
            {
                return string.Empty;
            }

            var originalUsernameClaim = claimsPrincipal.Claims.SingleOrDefault(c => c.Type == ConstDataLayer.ClaimOriginalUsername);

            if (originalUsernameClaim == null)
            {
                return string.Empty;
            }

            return originalUsernameClaim.Value;
        }
    }
}