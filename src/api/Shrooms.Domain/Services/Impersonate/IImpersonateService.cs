using System.Security.Claims;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Impersonate
{
    public interface IImpersonateService
    {
        Task<string> ImpersonateUserAsync(string userName, ClaimsPrincipal principal);
        Task<string> RevertImpersonationAsync(string originalUserName);
    }
}
