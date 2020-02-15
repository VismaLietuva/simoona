using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;

namespace Shrooms.Domain.Services.Impersonate
{
    public interface IImpersonateService
    {
        Task<string> ImpersonateUserAsync(string userName, OAuthAuthorizationServerOptions serverAuthOptions, ClaimsPrincipal principal);
        Task<string> RevertImpersonationAsync(string originalUserName, OAuthAuthorizationServerOptions serverAuthOptions);
    }
}
