using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Authentification.Membership;
using Shrooms.Domain.Services.Jwt;
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
        private readonly IJwtTokenService _jwtTokenService;

        public TokenController(ShroomsUserManager userManager, IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost]
        public async Task<IActionResult> Token()
        {
            // The SPA sends Content-Type: application/json but a form-encoded body.
            // Read the raw body and parse form fields regardless of Content-Type.
            Request.EnableBuffering();
            string body;
            using (var reader = new System.IO.StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
            }

            var form = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(body);
            var userName = form.TryGetValue("username", out var u) ? u.ToString() : null;
            var password = form.TryGetValue("password", out var pw) ? pw.ToString() : null;

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new { error = "invalid_request" });
            }

            var user = await _userManager.FindByNameAsync(userName)
                ?? await _userManager.FindByEmailAsync(userName);

            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
                return BadRequest(new { error = "invalid_grant", error_description = "The user name or password is incorrect" });
            }

            if (!user.EmailConfirmed)
            {
                return BadRequest(new { error = "not_verified", error_description = "E-mail address is not verified" });
            }

            var result = await _jwtTokenService.GenerateTokenAsync(user);

            return Ok(new
            {
                access_token = result.Token,
                token_type = "bearer",
                expires_in = result.ExpiresIn,
                userIdentifier = user.Id
            });
        }
    }
}
