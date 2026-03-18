using Shrooms.DataLayer.EntityModels.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Jwt
{
    public interface IJwtTokenService
    {
        Task<string> GenerateTokenAsync(ApplicationUser user, IEnumerable<Claim> extraClaims = null);
    }
}
