using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.Api.Helpers;
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
        private readonly IReturnUrlValidator _returnUrlValidator;
        private readonly IExternalEmailTrust _externalEmailTrust;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly ILogger<AccountController> _logger;

        private const string ChallengeProviderKey = "simoona.provider";
        private const string ChallengeOrganizationKey = "simoona.organization";

        private static readonly string[] ExternalProviders =
        {
            AuthenticationConstants.GoogleLoginProvider,
            AuthenticationConstants.FacebookLoginProvider,
            AuthenticationConstants.MicrosoftLoginProvider,
        };

        private string RequestedOrganization => HttpContext.GetRequestedTenant();

        public AccountController(
            IMapper mapper,
            ShroomsUserManager userManager,
            IPermissionService permissionService,
            IOrganizationService organizationService,
            IRefreshTokenService refreshTokenService,
            IAdministrationUsersService administrationService,
            IApplicationSettings applicationSettings,
            IJwtTokenService jwtTokenService,
            IReturnUrlValidator returnUrlValidator,
            IExternalEmailTrust externalEmailTrust,
            IAuthenticationSchemeProvider schemeProvider,
            ILogger<AccountController> logger)
        {
            _returnUrlValidator = returnUrlValidator;
            _externalEmailTrust = externalEmailTrust;
            _schemeProvider = schemeProvider;
            _logger = logger;
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
        [EnableRateLimiting(AuthRateLimit.PolicyName)]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Internal (password) accounts must be enabled for the organisation, and when it restricts
            // sign-ups to its own email domain that applies to internal registration too, not only social.
            var organization = await _organizationService.GetOrganizationByNameAsync(RequestedOrganization);
            if (!ContainsProvider(organization.AuthenticationProviders ?? string.Empty, AuthenticationConstants.InternalLoginProvider))
            {
                return BadRequest(new { error = "internal_registration_disabled" });
            }

            if (!await _organizationService.IsOrganizationHostValidAsync(model.Email, RequestedOrganization))
            {
                return BadRequest(new { error = "email_host_not_allowed" });
            }

            // Password rules are checked before the address is looked up, so a weak password gets the same
            // 400 whether or not the address is registered (otherwise the difference would reveal it).
            var passwordCheck = await ValidatePasswordAsync(model.Email, model.Password);
            if (!passwordCheck.Succeeded)
            {
                return GetErrorResult(passwordCheck);
            }

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                // The caller has not proven ownership of this address. A confirmed account is never modified.
                // An unconfirmed internal account takes the new password and gets a fresh verification email:
                // setting the password rotates the security stamp (invalidating every earlier link), and
                // VerifyEmail requires that same password, so only someone holding both the mailbox and the
                // credential can ever confirm. Repeated re-registration by a stranger is a nuisance (the
                // pending user re-registers again) but never a takeover. Both cases answer 200 so the
                // endpoint does not reveal which addresses exist.
                if (!existing.EmailConfirmed
                    && await _administrationService.HasExistingExternalLoginAsync(model.Email, AuthenticationConstants.InternalLoginProvider))
                {
                    if (await _userManager.HasPasswordAsync(existing))
                    {
                        await _userManager.RemovePasswordAsync(existing);
                    }

                    await _userManager.AddPasswordAsync(existing, model.Password);
                    await _administrationService.SendUserVerificationEmailAsync(existing, RequestedOrganization);
                }

                return Ok();
            }

            if (await _administrationService.UserIsSoftDeletedAsync(model.Email))
            {
                // Restoring a deleted account (with its previous roles) is an administrator action.
                _logger.LogInformation("Registration attempted for a deleted account in {Organization}; not restoring.", RequestedOrganization);
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
        [EnableRateLimiting(AuthRateLimit.PolicyName)]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
        [EnableRateLimiting(AuthRateLimit.PolicyName)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return InvalidTokenResult();
            }


            // Mailbox plus credential: whoever registered the address must also know its password to
            // confirm it. This is what makes anonymous re-registration safe: an attacker can set a
            // password on a pending account but can never confirm it, and the real owner can never be
            // tricked into confirming a password they did not choose. Same response as a bad code.
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return InvalidTokenResult();
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
        [EnableRateLimiting(AuthRateLimit.PolicyName)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return InvalidTokenResult();
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            // The owner proved control of the mailbox; a lockout caused by someone else's guesses ends here.
            if (_userManager.SupportsUserLockout)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("InternalLogins")]
        [ProducesResponseType(typeof(List<ExternalLoginViewModel>), StatusCodes.Status200OK)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(List<ExternalLoginViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetExternalLogins(string returnUrl, bool isLinkable = false)
        {
            if (!_returnUrlValidator.IsAllowed(returnUrl))
            {
                return BadRequest("returnUrl must point to a configured client origin.");
            }

            var logins = new List<ExternalLoginViewModel>();
            var organizationProviders = (await _organizationService.GetOrganizationByNameAsync(RequestedOrganization)).AuthenticationProviders;

            if (string.IsNullOrEmpty(organizationProviders))
            {
                return Ok(logins);
            }

            foreach (var provider in ExternalProviders)
            {
                // Only providers the organisation enabled AND whose scheme is actually registered (i.e. the
                // credentials are configured); otherwise the button would lead to a 400 from ExternalLogin.
                if (!ContainsProvider(organizationProviders, provider) || !await IsKnownExternalProviderAsync(provider))
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
        public async Task<IActionResult> ExternalLogin(string provider, string organization, string returnUrl, bool isRegistration = false)
        {
            if (string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(returnUrl))
            {
                return BadRequest();
            }

            // The callback appends the access token to returnUrl as a fragment, so only origins we
            // configured may ever receive it. Checked here, before the IdP round-trip, and again in the
            // callback because both parameters travel through the IdP redirect.
            if (!_returnUrlValidator.IsAllowed(returnUrl))
            {
                return BadRequest("returnUrl must point to a configured client origin.");
            }

            if (!await IsKnownExternalProviderAsync(provider) || !await IsProviderEnabledForOrganizationAsync(provider, organization))
            {
                return BadRequest("Unknown authentication provider.");
            }

            var callback = Url.Action(nameof(ExternalLoginCallback), "Account", new
            {
                provider,
                organization,
                returnUrl,
                isRegistration
            });

            // The provider and organisation ride inside the authentication properties, which the remote
            // handler carries through its state parameter and into the external cookie, so the callback can
            // check that the principal really came from this provider for this tenant.
            var props = new AuthenticationProperties { RedirectUri = callback };
            props.Items[ChallengeProviderKey] = provider;
            props.Items[ChallengeOrganizationKey] = organization;
            return Challenge(props, provider);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ExternalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback(string provider, string organization, string returnUrl, bool isRegistration = false)
        {
            if (string.IsNullOrEmpty(returnUrl) || string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(organization))
            {
                return BadRequest();
            }

            if (!_returnUrlValidator.IsAllowed(returnUrl)
                || !await IsKnownExternalProviderAsync(provider)
                || !await IsProviderEnabledForOrganizationAsync(provider, organization))
            {
                return BadRequest();
            }

            // Pull the external identity that the social handler stashed in the ExternalScheme cookie.
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (!result.Succeeded || result.Principal == null)
            {
                return Redirect(AppendHash(returnUrl, "error=external_auth_failed"));
            }

            // All social handlers sign into the same external cookie. The query string names a provider and
            // tenant, but the cookie records which scheme actually authenticated and for which tenant the
            // challenge was issued; they must match, otherwise a Google principal could be attached as a
            // Facebook login, or a login issued for one tenant replayed against another.
            var items = result.Properties?.Items;
            if (items == null
                || !items.TryGetValue(".AuthScheme", out var authenticatedScheme) || !string.Equals(authenticatedScheme, provider, StringComparison.Ordinal)
                || !items.TryGetValue(ChallengeProviderKey, out var challengedProvider) || !string.Equals(challengedProvider, provider, StringComparison.Ordinal)
                || !items.TryGetValue(ChallengeOrganizationKey, out var challengedOrganization) || !string.Equals(challengedOrganization, organization, StringComparison.OrdinalIgnoreCase))
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                return BadRequest();
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
                // From here on the email decides which account this login becomes, so it must be one the identity
                // provider vouches for. Microsoft outside the trusted tenants, or Google without email_verified,
                // can carry an address chosen by whoever controls the account: matching it would hand over the
                // account that owns the address, and registering with it would plant a member with a company
                // address nobody at the company owns (ready to be "linked" by the real owner later).
                if (!_externalEmailTrust.IsEmailVerified(provider, result.Principal))
                {
                    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                    return Redirect(AppendHash(returnUrl, "error=email_not_verified"));
                }

                var existing = await _userManager.FindByEmailAsync(email);

                if (existing != null)
                {
                    // Email exists in this tenant: attach the social login to the existing user. If that account
                    // was never confirmed, whoever registered it could not prove they own the address, so its
                    // password is discarded; the identity provider has just verified the email, so confirm it.
                    if (!existing.EmailConfirmed)
                    {
                        if (await _userManager.HasPasswordAsync(existing))
                        {
                            await _userManager.RemovePasswordAsync(existing);
                        }

                        existing.EmailConfirmed = true;
                        await _userManager.UpdateAsync(existing);
                    }

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

        // Only the social providers Simoona knows, and only when the scheme is actually registered
        // (a provider without configured credentials is not added at startup).
        private async Task<bool> IsKnownExternalProviderAsync(string provider)
        {
            if (!ExternalProviders.Contains(provider, StringComparer.Ordinal))
            {
                return false;
            }

            return await _schemeProvider.GetSchemeAsync(provider) != null;
        }

        private async Task<IdentityResult> ValidatePasswordAsync(string email, string password)
        {
            var probe = new ApplicationUser { UserName = email, Email = email };
            var errors = new List<IdentityError>();
            foreach (var validator in _userManager.PasswordValidators)
            {
                var result = await validator.ValidateAsync(_userManager, probe, password);
                if (!result.Succeeded)
                {
                    errors.AddRange(result.Errors);
                }
            }

            return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray());
        }

        private async Task<bool> IsProviderEnabledForOrganizationAsync(string provider, string organizationName)
        {
            if (string.IsNullOrEmpty(organizationName))
            {
                return false;
            }

            Organization organization;
            try
            {
                organization = await _organizationService.GetOrganizationByNameAsync(organizationName);
            }
            catch (InvalidOperationException)
            {
                // Configured tenant without a matching Organizations row: not enabled, not a 500.
                return false;
            }

            return organization != null && ContainsProvider(organization.AuthenticationProviders ?? string.Empty, provider);
        }

        private static string AppendHash(string url, string hash)
        {
            if (string.IsNullOrEmpty(hash)) return url;
            var sep = url.Contains('#') ? "&" : "#";
            return url + sep + hash;
        }

        // The setting is a delimited list ("internal;google;facebook"); compare whole tokens so that a value
        // such as "notgoogle" cannot enable Google.
        private static bool ContainsProvider(string providerList, string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerList) || string.IsNullOrWhiteSpace(providerName))
            {
                return false;
            }

            return providerList
                .Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(token => string.Equals(token, providerName, StringComparison.OrdinalIgnoreCase));
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

        // Same shape as a failed ConfirmEmailAsync/ResetPasswordAsync, so an unknown address cannot be
        // told apart from a bad code.
        private IActionResult InvalidTokenResult()
        {
            return GetErrorResult(IdentityResult.Failed((_userManager.ErrorDescriber ?? new IdentityErrorDescriber()).InvalidToken()));
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
