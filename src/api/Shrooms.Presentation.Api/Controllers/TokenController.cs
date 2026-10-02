using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Shrooms.Authentification.Membership;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Jwt;
using Shrooms.Presentation.Api.Filters;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Presentation.Api.Controllers
{
    [AllowAnonymous]
    [Route("token")]
    [ApiController]
    [EnableRateLimiting(AuthRateLimit.PolicyName)]
    public class TokenController : ControllerBase
    {
        private readonly ShroomsUserManager _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<TokenController> _logger;
        private readonly ShroomsDbContext _dbContext;

        public TokenController(ShroomsUserManager userManager, IJwtTokenService jwtTokenService, ILogger<TokenController> logger, ShroomsDbContext dbContext)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
            _dbContext = dbContext;
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

            if (user == null)
            {
                return InvalidCredentials();
            }

            var lockoutSupported = _userManager.SupportsUserLockout;

            // Locked accounts are refused before the password is checked, so a locked-out attacker
            // learns nothing about the password and the failure counter keeps the lockout alive.
            if (lockoutSupported && await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Login refused for locked-out user {UserId} from {Ip}", user.Id, HttpContext.Connection.RemoteIpAddress);
                return BadRequest(new { error = "account_locked", error_description = "Account is temporarily locked because of too many failed sign-in attempts. Try again later." });
            }

            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                if (lockoutSupported)
                {
                    await RecordFailedAttemptAsync(user);

                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        _logger.LogWarning("User {UserId} locked out after repeated failed sign-in attempts from {Ip}", user.Id, HttpContext.Connection.RemoteIpAddress);
                    }
                }

                return InvalidCredentials();
            }

            if (!user.EmailConfirmed)
            {
                return BadRequest(new { error = "not_verified", error_description = "E-mail address is not verified" });
            }

            if (lockoutSupported)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
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

        // Parallel wrong guesses race on the user's concurrency stamp and Identity reports the losers as
        // ConcurrencyFailure, which would let a burst of attempts count as one. The entity is tracked by this
        // request's context, so FindByIdAsync would hand back the same stale object; reload it from the
        // database and retry so every attempt is recorded.
        private async Task RecordFailedAttemptAsync(ApplicationUser user)
        {
            for (var attempt = 0; attempt < 5; attempt++)
            {
                var result = await _userManager.AccessFailedAsync(user);
                if (result.Succeeded)
                {
                    return;
                }

                await _dbContext.Entry(user).ReloadAsync();
                if (_dbContext.Entry(user).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                {
                    return;
                }

                // Another attempt may already have locked the account while we retried.
                if (await _userManager.IsLockedOutAsync(user))
                {
                    return;
                }
            }

            _logger.LogWarning("Could not record a failed sign-in attempt for user {UserId} after repeated concurrency conflicts", user.Id);
        }

        private IActionResult InvalidCredentials()
        {
            return BadRequest(new { error = "invalid_grant", error_description = "The user name or password is incorrect" });
        }
    }
}
