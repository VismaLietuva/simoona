using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Authentification.Membership
{
    public class ShroomsClaimsIdentityFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
    {
        private readonly IDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShroomsClaimsIdentityFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            IDbContext context,
            IHttpContextAccessor httpContextAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var claimsIdentity = await base.GenerateClaimsAsync(user);
            var contextUser = _httpContextAccessor.HttpContext?.User;
            
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

            return claimsIdentity;
        }

        private Organization GetOrganization(int? orgId)
        {
            return _context.Set<Organization>().FirstOrDefault(u => u.Id == orgId);
        }
    }
}