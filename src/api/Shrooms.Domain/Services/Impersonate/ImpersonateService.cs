using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Shrooms.Authentification.Membership;

namespace Shrooms.Domain.Services.Impersonate
{
    public class ImpersonateService : IImpersonateService
    {
        private readonly ShroomsUserManager _userManager;

        public ImpersonateService(ShroomsUserManager userManager)
        {
            _userManager = userManager;
        }

        // TODO: Complete migration to ASP.NET Core authentication tokens
        public async Task<string> ImpersonateUserAsync(string userName, object serverAuthOptions, ClaimsPrincipal principal)
        {
            throw new NotImplementedException("OWIN OAuth migration to ASP.NET Core authentication pending");
        }

        public async Task<string> RevertImpersonationAsync(string originalUserName, object serverAuthOptions)
        {
            throw new NotImplementedException("OWIN OAuth migration to ASP.NET Core authentication pending");
        }
    }
}
