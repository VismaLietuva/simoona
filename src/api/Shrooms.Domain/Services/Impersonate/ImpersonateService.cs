using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.ServiceExceptions;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Impersonate
{
    public class ImpersonateService : IImpersonateService
    {
        private readonly ShroomsUserManager _userManager;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork2 _uow;

        public ImpersonateService(ShroomsUserManager userManager, IConfiguration configuration, IUnitOfWork2 uow)
        {
            _userManager = userManager;
            _configuration = configuration;
            _uow = uow;
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

            return await GenerateJwtTokenAsync(targetUser, extraClaims);
        }

        public async Task<string> RevertImpersonationAsync(string originalUserName)
        {
            if (string.IsNullOrEmpty(originalUserName))
            {
                throw new ServiceException("Original username must not be empty. Revert is only valid after an active impersonation.");
            }

            var originalUser = await _userManager.FindByNameAsync(originalUserName);
            if (originalUser == null)
            {
                throw new ServiceException($"User '{originalUserName}' was not found.");
            }

            // Return a clean token with no impersonation claims
            return await GenerateJwtTokenAsync(originalUser, extraClaims: null);
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user, IEnumerable<Claim> extraClaims)
        {
            var jwtKey = _configuration["JwtSecret"]
                ?? throw new InvalidOperationException("JwtSecret is not configured.");
            var hours = int.TryParse(_configuration["AccessTokenLifeTimeInHours"], out var h) ? h : 24;

            var roles = await _userManager.GetRolesAsync(user);

            var orgShortName = await _uow.GetDbSet<Organization>()
                .Where(o => o.Id == user.OrganizationId)
                .Select(o => o.ShortName)
                .FirstOrDefaultAsync() ?? user.OrganizationId.ToString();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(WebApiConstants.ClaimOrganizationId, user.OrganizationId.ToString()),
                new Claim(WebApiConstants.ClaimOrganizationName, orgShortName),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            if (extraClaims != null)
            {
                claims.AddRange(extraClaims);
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(hours);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
