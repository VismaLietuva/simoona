using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;

namespace Shrooms.Authentification.Membership
{
    public class ShroomsClaimsIdentityFactory : ClaimsIdentityFactory<ApplicationUser, string>
    {
        private readonly IDbContext _context;

        public ShroomsClaimsIdentityFactory(IDbContext context)
        {
            _context = context;
        }

        public override Task<ClaimsIdentity> CreateAsync(UserManager<ApplicationUser, string> userManager, ApplicationUser user, string authenticationType)
        {
            var contextUser = HttpContext.Current.User as ClaimsPrincipal;
            var claimsIdentity = base.CreateAsync(userManager, user, authenticationType).Result;
            var organizationIdClaim = new Claim(WebApiConstants.ClaimOrganizationId, user.OrganizationId.ToString());

            if (!claimsIdentity.HasClaim(claim => claim.Type == ClaimTypes.GivenName))
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}"));
            }

            if (!claimsIdentity.HasClaim(organizationIdClaim.Type, organizationIdClaim.Value))
            {
                claimsIdentity.AddClaim(organizationIdClaim);
            }

            var organizationNameClaim = new Claim(WebApiConstants.ClaimOrganizationName, GetOrganization(user.OrganizationId).ShortName);
            if (!claimsIdentity.HasClaim(organizationNameClaim.Type, organizationNameClaim.Value))
            {
                claimsIdentity.AddClaim(organizationNameClaim);
            }

            //if user is impersonated add additional claims
            if (contextUser != null && contextUser.Claims.Any(c => c.Type == WebApiConstants.ClaimUserImpersonation && c.Value == true.ToString()) && contextUser.Claims.First(c => c.Type == WebApiConstants.ClaimOriginalUsername).Value != user.UserName)
            {
                claimsIdentity.AddClaim(contextUser.Claims.FirstOrDefault(c => c.Type == WebApiConstants.ClaimUserImpersonation));
                claimsIdentity.AddClaim(contextUser.Claims.FirstOrDefault(c => c.Type == WebApiConstants.ClaimOriginalUsername));
                claimsIdentity.AddClaim(contextUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid));
            }

            return Task.FromResult(claimsIdentity);
        }

        private Organization GetOrganization(int? orgId)
        {
            return _context.Set<Organization>().FirstOrDefault(u => u.Id == orgId);
        }
    }
}