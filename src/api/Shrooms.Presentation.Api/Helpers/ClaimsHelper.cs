using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Shrooms.Contracts.Constants;

namespace Shrooms.Presentation.Api.Helpers
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
            var orgIdClaim = claimsIdentity?.FindFirst(WebApiConstants.ClaimOrganizationId);

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
            var orgNameClaim = claimsIdentity?.FindFirst(WebApiConstants.ClaimOrganizationName);
            if (orgNameClaim == null)
            {
                return string.Empty;
            }

            return orgNameClaim.Value;
        }

        public static string GetOriginalUsername(this IPrincipal principal)
        {
            var claimsPrincipal = principal as ClaimsPrincipal;
            if (claimsPrincipal == null)
            {
                return string.Empty;
            }

            if (!claimsPrincipal.HasClaim(DataLayerConstants.ClaimUserImpersonation, true.ToString()))
            {
                return string.Empty;
            }

            var originalUsernameClaim = claimsPrincipal.Claims.SingleOrDefault(c => c.Type == DataLayerConstants.ClaimOriginalUsername);

            if (originalUsernameClaim == null)
            {
                return string.Empty;
            }

            return originalUsernameClaim.Value;
        }
    }
}