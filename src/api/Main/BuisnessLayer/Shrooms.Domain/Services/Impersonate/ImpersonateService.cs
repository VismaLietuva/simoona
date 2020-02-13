using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Shrooms.Authentification.Membership;
using Shrooms.Host.Contracts.Constants;
using OwinDate = Microsoft.Owin.Infrastructure;

namespace Shrooms.Domain.Services.Impersonate
{
    public class ImpersonateService : IImpersonateService
    {
        private readonly ShroomsUserManager _userManager;

        public ImpersonateService(ShroomsUserManager userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> ImpersonateUserAsync(string userName, OAuthAuthorizationServerOptions serverAuthOptions, ClaimsPrincipal principal)
        {
            var originalUsername = principal.Claims.Any(c => c.Type == DataLayerConstants.ClaimUserImpersonation && c.Value == true.ToString()) ? principal.Claims.First(c => c.Type == DataLayerConstants.ClaimOriginalUsername).Value : principal.Identity.Name;
            var impersonatedUser = await _userManager.FindByNameAsync(userName);
            var impersonatedIdentity = await _userManager.CreateIdentityAsync(impersonatedUser, OAuthDefaults.AuthenticationType);

            if (impersonatedUser.UserName != originalUsername)
            {
                if (impersonatedIdentity.Claims.Any(c => c.Type == DataLayerConstants.ClaimUserImpersonation && c.Value == true.ToString()))
                {
                    var primarySidClaim = impersonatedIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid);
                    impersonatedIdentity.RemoveClaim(primarySidClaim);
                    impersonatedIdentity.AddClaim(new Claim(ClaimTypes.PrimarySid, string.Empty));
                }
                else
                {
                    impersonatedIdentity.AddClaim(new Claim(DataLayerConstants.ClaimUserImpersonation, true.ToString()));
                    impersonatedIdentity.AddClaim(new Claim(DataLayerConstants.ClaimOriginalUsername, originalUsername));

                    impersonatedIdentity.AddClaim(new Claim(ClaimTypes.PrimarySid, string.Empty));
                }
            }

            var ticket = new AuthenticationTicket(impersonatedIdentity, new AuthenticationProperties());
            var currentUtc = new OwinDate.SystemClock().UtcNow;
            ticket.Properties.IssuedUtc = currentUtc;
            ticket.Properties.ExpiresUtc = currentUtc.Add(serverAuthOptions.AccessTokenExpireTimeSpan);
            return serverAuthOptions.AccessTokenFormat.Protect(ticket);
        }

        public async Task<string> RevertImpersonationAsync(string originalUserName, OAuthAuthorizationServerOptions serverAuthOptions)
        {
            var originalUser = await _userManager.FindByNameAsync(originalUserName);

            var impersonatedIdentity = await _userManager.CreateIdentityAsync(originalUser, OAuthDefaults.AuthenticationType);
            impersonatedIdentity.AddClaim(new Claim(ClaimTypes.PrimarySid, string.Empty));

            var ticket = new AuthenticationTicket(impersonatedIdentity, new AuthenticationProperties());
            var currentUtc = new OwinDate.SystemClock().UtcNow;
            ticket.Properties.IssuedUtc = currentUtc;
            ticket.Properties.ExpiresUtc = currentUtc.Add(serverAuthOptions.AccessTokenExpireTimeSpan);

            return serverAuthOptions.AccessTokenFormat.Protect(ticket);
        }
    }
}
