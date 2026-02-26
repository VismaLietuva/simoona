using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.DataLayer.DAL;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Presentation.Api.Controllers
{
    [AllowAnonymous]
    [Route("token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ShroomsUserManager _userManager;
        private readonly IConfiguration _configuration;
        private readonly ShroomsDbContext _dbContext;

        public TokenController(ShroomsUserManager userManager, IConfiguration configuration, ShroomsDbContext dbContext)
        {
            _userManager = userManager;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Token([FromForm] LoginRequestModel model)
        {
            if (string.IsNullOrEmpty(model?.UserName) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new { error = "invalid_request" });
            }

            var user = await _userManager.FindByNameAsync(model.UserName)
                ?? await _userManager.FindByEmailAsync(model.UserName);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return BadRequest(new { error = "invalid_grant", error_description = "The user name or password is incorrect" });
            }

            if (!user.EmailConfirmed)
            {
                return BadRequest(new { error = "not_verified", error_description = "E-mail address is not verified" });
            }

            var token = await GenerateJwtTokenAsync(user);
            return Ok(token);
        }

        private async Task<object> GenerateJwtTokenAsync(Shrooms.DataLayer.EntityModels.Models.ApplicationUser user)
        {
            var jwtKey = _configuration["JwtSecret"] ?? "default-secret-key-change-in-production-min32chars!!";
            var hours = int.TryParse(_configuration["AccessTokenLifeTimeInHours"], out var h) ? h : 24;

            var roles = await _userManager.GetRolesAsync(user);

            var orgShortName = await _dbContext.Set<Shrooms.DataLayer.EntityModels.Models.Organization>()
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

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(hours);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new
            {
                access_token = new JwtSecurityTokenHandler().WriteToken(token),
                token_type = "bearer",
                expires_in = (int)TimeSpan.FromHours(hours).TotalSeconds,
                userIdentifier = user.Id
            };
        }
    }

    public class LoginRequestModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
    }
}
