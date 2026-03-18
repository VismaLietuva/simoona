using Shrooms.DataLayer.EntityModels.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Jwt
{
    public record JwtTokenResult(string Token, int ExpiresIn);

    public interface IJwtTokenService
    {
        Task<JwtTokenResult> GenerateTokenAsync(ApplicationUser user, IEnumerable<Claim> extraClaims = null);
    }
}
