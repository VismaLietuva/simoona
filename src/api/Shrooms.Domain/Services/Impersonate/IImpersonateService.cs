using System.Security.Claims;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Impersonate
{
    public interface IImpersonateService
    {
        // TODO: Migrate to ASP.NET Core authentication
        Task<string> ImpersonateUserAsync(string userName, object serverAuthOptions, ClaimsPrincipal principal);
        Task<string> RevertImpersonationAsync(string originalUserName, object serverAuthOptions);
    }
}
