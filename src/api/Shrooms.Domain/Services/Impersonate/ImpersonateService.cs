using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Domain.ServiceExceptions;
using Shrooms.Domain.Services.Jwt;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Impersonate
{
    public class ImpersonateService : IImpersonateService
    {
        private readonly ShroomsUserManager _userManager;
        private readonly IJwtTokenService _jwtTokenService;

        public ImpersonateService(ShroomsUserManager userManager, IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<string> ImpersonateUserAsync(string userName, ClaimsPrincipal principal)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ServiceException("Target username must not be empty.");
            }

            if (principal?.Identity == null || !principal.Identity.IsAuthenticated || string.IsNullOrEmpty(principal.Identity.Name))
            {
                throw new ServiceException("Caller principal must be authenticated.");
            }

            var targetUser = await _userManager.FindByNameAsync(userName);
            if (targetUser == null)
            {
                throw new ServiceException($"User '{userName}' was not found.");
            }

            var extraClaims = new List<Claim>
            {
                new Claim(DataLayerConstants.ClaimUserImpersonation, true.ToString()),
                new Claim(DataLayerConstants.ClaimOriginalUsername, principal.Identity.Name)
            };

            return (await _jwtTokenService.GenerateTokenAsync(targetUser, extraClaims)).Token;
        }

        public async Task<string> RevertImpersonationAsync(ClaimsPrincipal principal)
        {
            if (principal == null || !principal.HasClaim(DataLayerConstants.ClaimUserImpersonation, true.ToString()))
            {
                throw new ServiceException("Revert is only valid during an active impersonation session.");
            }

            var originalUserName = principal.FindFirstValue(DataLayerConstants.ClaimOriginalUsername);
            if (string.IsNullOrEmpty(originalUserName))
            {
                throw new ServiceException("Original username claim is missing from the impersonation token.");
            }

            var originalUser = await _userManager.FindByNameAsync(originalUserName);
            if (originalUser == null)
            {
                throw new ServiceException($"User '{originalUserName}' was not found.");
            }

            return (await _jwtTokenService.GenerateTokenAsync(originalUser)).Token;
        }
    }
}
