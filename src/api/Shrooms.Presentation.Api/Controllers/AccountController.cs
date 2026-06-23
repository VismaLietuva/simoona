using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Administration;
using Shrooms.Domain.Services.Jwt;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.RefreshTokens;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Helpers;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.AccountModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [Route("Account")]
    public class AccountController : BaseController
    {
        private readonly ShroomsUserManager _userManager;
        private readonly IMapper _mapper;
        private readonly IPermissionService _permissionService;
        private readonly IOrganizationService _organizationService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IAdministrationUsersService _administrationService;
        private readonly IApplicationSettings _applicationSettings;
        private readonly IJwtTokenService _jwtTokenService;

        private string RequestedOrganization => HttpContext.GetRequestedTenant();

        public AccountController(
            IMapper mapper,
            ShroomsUserManager userManager,
            IPermissionService permissionService,
            IOrganizationService organizationService,
            IRefreshTokenService refreshTokenService,
            IAdministrationUsersService administrationService,
            IApplicationSettings applicationSettings,
            IJwtTokenService jwtTokenService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _permissionService = permissionService;
            _organizationService = organizationService;
            _refreshTokenService = refreshTokenService;
            _administrationService = administrationService;
            _applicationSettings = applicationSettings;
            _jwtTokenService = jwtTokenService;
        }

        [Route("UserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Ok(new ExternalUserInfoViewModel { HasRegistered = false });
            }

            try
            {
                var loggedUser = await GetLoggedInUserInfoAsync();
                return Ok(loggedUser);
            }
            catch (InvalidOperationException)
            {
                return Unauthorized();
            }
        }

        [AllowAnonymous]
        [Route("Register")]
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _administrationService.UserEmailExistsAsync(model.Email))
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null || user.EmailConfirmed || !await _administrationService.HasExistingExternalLoginAsync(model.Email, AuthenticationConstants.InternalLoginProvider))
                {
                    return BadRequest("User already exists");
                }

                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, model.Password);
                await _administrationService.SendUserVerificationEmailAsync(user, RequestedOrganization);

                return Ok();
            }

            if (await _administrationService.UserIsSoftDeletedAsync(model.Email))
            {
                await _administrationService.RestoreUserAsync(model.Email);
                return Ok();
            }

            var result = await _administrationService.CreateNewUserAsync(_mapper.Map<ApplicationUser>(model), model.Password, RequestedOrganization);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("RequestPasswordReset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return Ok();
            }

            await _administrationService.SendUserPasswordResetEmailAsync(user, RequestedOrganization);

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return BadRequest();
            }

            var result = await _userManager.ConfirmEmailAsync(user, model.Code);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return BadRequest();
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("InternalLogins")]
        public async Task<IActionResult> GetInternalLogins()
        {
            var logins = new List<ExternalLoginViewModel>();
            var organizationProviders = (await _organizationService.GetOrganizationByNameAsync(RequestedOrganization)).AuthenticationProviders;

            if (!ContainsProvider(organizationProviders, AuthenticationConstants.InternalLoginProvider))
            {
                return Ok(logins);
            }

            logins.Add(new ExternalLoginViewModel { Name = AuthenticationConstants.InternalLoginProvider });

            return Ok(logins);
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Ok();
            }

            var userAndOrganization = GetUserAndOrganization();
            await _refreshTokenService.RemoveTokenBySubjectAsync(userAndOrganization);
            _permissionService.RemoveCache(userAndOrganization.UserId);

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ExternalLogins")]
        public async Task<IActionResult> GetExternalLogins(string returnUrl, bool isLinkable = false)
        {
            var logins = new List<ExternalLoginViewModel>();
            var organizationProviders = (await _organizationService.GetOrganizationByNameAsync(RequestedOrganization)).AuthenticationProviders;

            if (string.IsNullOrEmpty(organizationProviders))
            {
                return Ok(logins);
            }

            var externalProviders = new[]
            {
                AuthenticationConstants.GoogleLoginProvider,
                AuthenticationConstants.FacebookLoginProvider,
                AuthenticationConstants.MicrosoftLoginProvider,
            };

            foreach (var provider in externalProviders)
            {
                if (!ContainsProvider(organizationProviders, provider))
                {
                    continue;
                }

                logins.Add(new ExternalLoginViewModel
                {
                    Name = provider,
                    Url = BuildExternalLoginUrl(provider, RequestedOrganization, returnUrl, isRegistration: false)
                });

                logins.Add(new ExternalLoginViewModel
                {
                    Name = provider + "Registration",
                    Url = BuildExternalLoginUrl(provider, RequestedOrganization, returnUrl, isRegistration: true)
                });
            }

            return Ok(logins);
        }

        // Initiates the external OAuth challenge. The frontend opens this URL in the browser
        // (built by GetExternalLogins above); we round-trip the organization / returnUrl /
        // isRegistration through ExternalLoginCallback because the SignIn cookie can't carry
        // tenant context across IdP hops.
        [AllowAnonymous]
        [HttpGet]
        [Route("ExternalLogin")]
        public IActionResult ExternalLogin(string provider, string organization, string returnUrl, bool isRegistration = false)
        {
            if (string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(returnUrl))
            {
                return BadRequest();
            }

            var callback = Url.Action(nameof(ExternalLoginCallback), "Account", new
            {
                provider,
                organization,
                returnUrl,
                isRegistration
            });

            var props = new AuthenticationProperties { RedirectUri = callback };
            return Challenge(props, provider);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ExternalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback(string provider, string organization, string returnUrl, bool isRegistration = false)
        {
            if (string.IsNullOrEmpty(returnUrl) || string.IsNullOrEmpty(provider))
            {
                return BadRequest();
            }

            // Pull the external identity that the social handler stashed in the ExternalScheme cookie.
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (!result.Succeeded || result.Principal == null)
            {
                return Redirect(AppendHash(returnUrl, "error=external_auth_failed"));
            }

            var providerKey = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var loginProvider = provider;
            var email = result.Principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerKey))
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                return Redirect(AppendHash(returnUrl, "error=missing_claims"));
            }

            // Push the tenant into HttpContext.Items so the per-request DbContext (Program.cs:31)
            // resolves the right tenant's connection string for the lookups below.
            HttpContext.Items["tenantName"] = organization;

            var loginInfo = new ExternalLoginInfo(result.Principal, loginProvider, providerKey, loginProvider);
            var user = await _userManager.FindByLoginAsync(loginProvider, providerKey);

            if (user == null)
            {
                var existing = await _userManager.FindByEmailAsync(email);

                if (existing != null)
                {
                    // Email exists in this tenant: attach the social login to the existing user.
                    await _userManager.AddLoginAsync(existing, new UserLoginInfo(loginProvider, providerKey, loginProvider));
                    user = existing;
                }
                else if (isRegistration)
                {
                    // First-time external registration.
                    var isHostValid = await _organizationService.IsOrganizationHostValidAsync(email, organization);
                    if (!isHostValid)
                    {
                        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                        return Redirect(AppendHash(returnUrl, "error=invalid_email_host"));
                    }

                    var createResult = await _administrationService.CreateNewUserWithExternalLoginAsync(loginInfo, organization);
                    if (!createResult.Succeeded)
                    {
                        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                        return Redirect(AppendHash(returnUrl, "error=create_failed"));
                    }

                    user = await _userManager.FindByEmailAsync(email);
                }
                else
                {
                    // No matching user and not in registration mode → bounce back so the SPA can prompt registration.
                    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                    return Redirect(AppendHash(returnUrl, "error=user_not_found"));
                }
            }

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var token = await _jwtTokenService.GenerateTokenAsync(user);
            var hash = $"access_token={Uri.EscapeDataString(token.Token)}&token_type=bearer&expires_in={token.ExpiresIn}";
            return Redirect(AppendHash(returnUrl, hash));
        }

        private string BuildExternalLoginUrl(string provider, string organization, string returnUrl, bool isRegistration)
        {
            var qs = new Dictionary<string, string>
            {
                ["provider"] = provider,
                ["organization"] = organization ?? string.Empty,
                ["returnUrl"] = returnUrl ?? string.Empty,
                ["isRegistration"] = isRegistration ? "true" : "false"
            };
            return QueryHelpers.AddQueryString("/Account/ExternalLogin", qs);
        }

        private static string AppendHash(string url, string hash)
        {
            if (string.IsNullOrEmpty(hash)) return url;
            var sep = url.Contains('#') ? "&" : "#";
            return url + sep + hash;
        }

        private static bool ContainsProvider(string providerList, string providerName)
        {
            return providerList.ToLower().Contains(providerName.ToLower());
        }

        private async Task<LoggedInUserInfoViewModel> GetLoggedInUserInfoAsync()
        {
            var userId = User.Identity.GetUserId();
            var organizationId = User.Identity.GetOrganizationId();
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"Authenticated user '{userId}' not found in the database. The token may be stale.");
            }

            var permissions = await _permissionService.GetUserPermissionsAsync(userId, organizationId);

            var userInfo = new LoggedInUserInfoViewModel
            {
                HasRegistered = true,
                Roles = await _userManager.GetRolesAsync(user),
                UserName = User.Identity.Name,
                UserId = userId,
                OrganizationName = User.FindFirstValue(WebApiConstants.ClaimOrganizationName),
                OrganizationId = User.FindFirstValue(WebApiConstants.ClaimOrganizationId),
                FullName = User.FindFirstValue(ClaimTypes.GivenName),
                Permissions = permissions,
                Impersonated = claimsIdentity?.Claims.Any(c => c.Type == WebApiConstants.ClaimUserImpersonation && c.Value == true.ToString()) ?? false,
                CultureCode = user?.CultureCode,
                TimeZone = user?.TimeZone,
                PictureId = user?.PictureId
            };

            return userInfo;
        }

        private IActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return StatusCode(500);
            }

            if (result.Succeeded)
            {
                return null;
            }

            if (result.Errors != null)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            if (ModelState.IsValid)
            {
                return BadRequest();
            }

            return BadRequest(ModelState);
        }
    }
}
