using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Shrooms.Authentification.ExternalLoginInfrastructure;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Administration;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.RefreshTokens;
using Shrooms.Presentation.Api.Helpers;
using Shrooms.Presentation.Api.Providers;
using Shrooms.Presentation.Api.Results;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Helpers;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.AccountModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [RoutePrefix("Account")]
    public class AccountController : BaseController
    {
        private const int StateStrengthInBits = 256;
        private readonly ShroomsUserManager _userManager;
        private readonly IMapper _mapper;
        private readonly IPermissionService _permissionService;
        private readonly IOrganizationService _organizationService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IAdministrationUsersService _administrationService;
        private readonly IApplicationSettings _applicationSettings;

        private IAuthenticationManager Authentication => Request.GetOwinContext().Authentication;

        private string RequestedOrganization => Request.GetRequestedTenant();

        public AccountController(
            IMapper mapper,
            ShroomsUserManager userManager,
            IPermissionService permissionService,
            IOrganizationService organizationService,
            IRefreshTokenService refreshTokenService,
            IAdministrationUsersService administrationService,
            IApplicationSettings applicationSettings)
        {
            _mapper = mapper;
            _userManager = userManager;
            _permissionService = permissionService;
            _organizationService = organizationService;
            _refreshTokenService = refreshTokenService;
            _administrationService = administrationService;
            _applicationSettings = applicationSettings;
        }

        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public async Task<IHttpActionResult> GetUserInfo()
        {
            var externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
            if (externalLogin == null && User.Identity.IsAuthenticated)
            {
                var loggedUser = await GetLoggedInUserInfoAsync();
                return Ok(loggedUser);
            }
            else
            {
                var externalUserInfo = new ExternalUserInfoViewModel
                {
                    Email = User.Identity.GetUserName(),
                    HasRegistered = externalLogin == null,
                    LoginProvider = externalLogin?.LoginProvider
                };
                return Ok(externalUserInfo);
            }
        }

        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> RegisterUser([FromBody] RegisterViewModel model)
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

                await _userManager.RemovePasswordAsync(user.Id);
                await _userManager.AddPasswordAsync(user.Id, model.Password);
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

        public async Task<IHttpActionResult> SignIn(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userManager.FindAsync(model.UserName, model.Password);

            if (user == null)
            {
                return BadRequest();
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var oAuthIdentity = await _userManager.CreateIdentityAsync(user, OAuthDefaults.AuthenticationType);
            var cookieIdentity = await _userManager.CreateIdentityAsync(user, CookieAuthenticationDefaults.AuthenticationType);

            var properties = await CreateInitialRefreshToken(model.ClientId, user, oAuthIdentity);

            SetCookieExpirationDateToAccessTokenLifeTime(properties);

            Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);

            await _userManager.AddLoginAsync(user.Id, new UserLoginInfo(AuthenticationConstants.InternalLoginProvider, user.Id));

            return Ok();
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> RequestPasswordReset(ForgotPasswordViewModel model)
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
        public async Task<IHttpActionResult> VerifyEmail([FromBody] VerifyEmailViewModel model)
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

            var result = await _userManager.ConfirmEmailAsync(user.Id, model.Code);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
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

            var result = await _userManager.ResetPasswordAsync(user.Id, model.Code, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        [AllowAnonymous]
        [Route("InternalLogins")]
        public async Task<IHttpActionResult> GetInternalLogins()
        {
            var logins = new List<ExternalLoginViewModel>();
            var organizationProviders = (await _organizationService.GetOrganizationByNameAsync(RequestedOrganization)).AuthenticationProviders;

            if (!ContainsProvider(organizationProviders, AuthenticationConstants.InternalLoginProvider))
            {
                return Ok(logins);
            }

            var internalLogin = new ExternalLoginViewModel
            {
                Name = AuthenticationConstants.InternalLoginProvider
            };

            logins.Add(internalLogin);

            return Ok(logins);
        }

        [Route("Logout")]
        public async Task<IHttpActionResult> Logout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Ok();
            }

            var userAndOrganization = GetUserAndOrganization();
            await _refreshTokenService.RemoveTokenBySubjectAsync(userAndOrganization);
            _permissionService.RemoveCache(userAndOrganization.UserId);
            Authentication.SignOut();

            return Ok();
        }

        [AllowAnonymous]
        [Route("ExternalLogins")]
        public async Task<IHttpActionResult> GetExternalLogins(string returnUrl, bool isLinkable = false)
        {
            var descriptions = Authentication.GetExternalAuthenticationTypes();
            var logins = new List<ExternalLoginViewModel>();

            var organizationProviders = (await _organizationService.GetOrganizationByNameAsync(RequestedOrganization)).AuthenticationProviders;

            foreach (var description in descriptions)
            {
                if (!ContainsProvider(organizationProviders, description.Caption))
                {
                    continue;
                }
                var state = RandomOAuthStateGenerator.Generate(StateStrengthInBits);

                var login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = CreateUrl(description, returnUrl, state, isLinkable, false),
                    State = state
                };

                logins.Add(login);

                state = RandomOAuthStateGenerator.Generate(StateStrengthInBits);
                login = new ExternalLoginViewModel
                {
                    Name = $"{description.Caption}Registration",
                    Url = CreateUrl(description, returnUrl, state, isLinkable, true),
                    State = state
                };

                logins.Add(login);
            }

            return Ok(logins);
        }

        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return InternalServerError();
            }

            if (!await _administrationService.UserEmailExistsAsync(info.Email))
            {
                if (await _administrationService.UserIsSoftDeletedAsync(info.Email))
                {
                    await _administrationService.RestoreUserAsync(info.Email);
                }
                else
                {
                    var requestedOrganization = RequestedOrganization;
                    var result = await _administrationService.CreateNewUserWithExternalLoginAsync(info, requestedOrganization);
                    if (!result.Succeeded)
                    {
                        return GetErrorResult(result);
                    }
                }
            }
            else if (await _administrationService.HasExistingExternalLoginAsync(info.Email, info.Login.LoginProvider))
            {
                var user = await _userManager.FindByEmailAsync(info.Email);
                await _administrationService.AddProviderImageAsync(user.Id, info.ExternalIdentity);
                return Ok("User already exists");
            }
            else if (await _administrationService.HasExistingExternalLoginAsync(info.Email, AuthenticationConstants.InternalLoginProvider))
            {
                var user = await _userManager.FindByEmailAsync(info.Email);
                if (user?.EmailConfirmed == false)
                {
                    await _userManager.RemoveLoginAsync(user.Id, new UserLoginInfo(AuthenticationConstants.InternalLoginProvider, user.Id));
                    await _userManager.RemovePasswordAsync(user.Id);
                }
            }

            var userId = (await _userManager.FindByEmailAsync(info.Email)).Id;
            await _userManager.AddLoginAsync(userId, info.Login);
            await _administrationService.AddProviderImageAsync(userId, info.ExternalIdentity);
            await _administrationService.AddProviderEmailAsync(userId, info.Login.LoginProvider, info.Email);

            return Ok();
        }

        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        // ReSharper disable once InconsistentNaming
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string client_Id = null, string userId = null, bool isRegistration = false, string error = null)
        {
            if (string.IsNullOrEmpty(client_Id) || error != null)
            {
                var uri = CreateErrorUri("error");
                return Redirect(uri);
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            var externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
            if (externalLogin.Email == null)
            {
                var uri = CreateErrorUri("emailError");
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return Redirect(uri);
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            var user = await _userManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey));
            var hasLogin = user != null;

            if (isRegistration && hasLogin == false)
            {
                var isEmailHostValid = await _organizationService.IsOrganizationHostValidAsync(externalLogin.Email, RequestedOrganization);
                if (!isEmailHostValid)
                {
                    var uri = CreateErrorUri("error");
                    Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                    return Redirect(uri);
                }
            }

            // Linking accounts.
            if (userId != null)
            {
                return await LinkAccountsAsync(externalLogin, userId);
            }

            // Registration process.
            if (isRegistration == true)
            {
                return await RegisterOrLoginAsync(user, externalLogin, client_Id, hasLogin);
            }

            // Login process.
            return await LoginAsync(user, externalLogin, client_Id, hasLogin);
        }

        private static bool ContainsProvider(string providerList, string providerName)
        {
            return providerList.ToLower().Contains(providerName.ToLower());
        }

        private async Task<AuthenticationProperties> CreateInitialRefreshToken(string clientId, ApplicationUser user, ClaimsIdentity oAuthIdentity)
        {
            var userOrganization = new UserAndOrganizationDto
            {
                OrganizationId = user.OrganizationId,
                UserId = user.Id
            };

            await _refreshTokenService.RemoveTokenBySubjectAsync(userOrganization);

            var properties = ApplicationOAuthProvider.CreateProperties(user.Id, clientId);

            var ticket = new AuthenticationTicket(oAuthIdentity, properties);
            var context = new AuthenticationTokenCreateContext(Request.GetOwinContext(), Startup.OAuthServerOptions.RefreshTokenFormat, ticket);

            await Startup.OAuthServerOptions.RefreshTokenProvider.CreateAsync(context);
            properties.Dictionary.Add("refresh_token", context.Token);
            return properties;
        }

        private async Task<LoggedInUserInfoViewModel> GetLoggedInUserInfoAsync()
        {
            var userId = User.Identity.GetUserId();
            var organizationId = User.Identity.GetOrganizationId();
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var user = await _userManager.FindByIdAsync(userId);
            var permissions = await _permissionService.GetUserPermissionsAsync(userId, organizationId);

            var userInfo = new LoggedInUserInfoViewModel
            {
                HasRegistered = true,
                Roles = await _userManager.GetRolesAsync(userId),
                UserName = User.Identity.Name,
                UserId = userId,
                OrganizationName = claimsIdentity.FindFirstValue(WebApiConstants.ClaimOrganizationName),
                OrganizationId = claimsIdentity.FindFirstValue(WebApiConstants.ClaimOrganizationId),
                FullName = claimsIdentity.FindFirstValue(ClaimTypes.GivenName),
                Permissions = permissions,
                Impersonated = claimsIdentity?.Claims.Any(c => c.Type == WebApiConstants.ClaimUserImpersonation && c.Value == true.ToString()) ?? false,
                CultureCode = user.CultureCode,
                TimeZone = user.TimeZone,
                PictureId = user.PictureId
            };

            return userInfo;
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (result.Succeeded)
            {
                return null;
            }

            if (result.Errors != null)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }

            if (ModelState.IsValid)
            {
                // No ModelState errors are available to send, so just return an empty BadRequest.
                return BadRequest();
            }

            return BadRequest(ModelState);
        }

        private string CreateUrl(AuthenticationDescription description, string returnUrl, string state, bool isLinkable, bool isRegistration)
        {
            var url = Url.RouteFromController("ExternalLogin",
                        ControllerContext.ControllerDescriptor.ControllerName,
                        new
                        {
                            provider = description.AuthenticationType,
                            organization = RequestedOrganization,
                            response_type = "token",
                            client_id = Startup.JsAppClientId,
                            redirect_uri = new Uri(Request.RequestUri, $"{returnUrl}?authType={description.AuthenticationType}").AbsoluteUri,
                            state = state,
                            userId = isLinkable ? GetUserAndOrganization().UserId : null,
                            isRegistration = isRegistration ? "true" : null
                        });
            return url;
        }

        private async Task<IHttpActionResult> LinkAccountsAsync(ExternalLoginData externalLogin, string userId)
        {
            var info = await Authentication.GetExternalLoginInfoAsync();
            if (await _userManager.AddLoginAsync(userId, info.Login) == null)
            {
                var uri = CreateErrorUri("error");
                return Redirect(uri);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = new ClaimsIdentity(externalLogin.GetClaims(), OAuthDefaults.AuthenticationType);
            Authentication.SignIn(identity);
            await _administrationService.AddProviderEmailAsync(userId, info.Login.LoginProvider, info.Email);

            return Ok();
        }

        private async Task<IHttpActionResult> RegisterOrLoginAsync(ApplicationUser user, ExternalLoginData externalLogin, string clientId, bool hasLogin)
        {
            if (hasLogin)
            {
                await UpdateCookiesAndLoginAsync(user, externalLogin, clientId);
            }
            else if (await _administrationService.UserEmailExistsAsync(externalLogin.Email))
            {
                if (await _administrationService.HasExistingExternalLoginAsync(externalLogin.Email, externalLogin.LoginProvider))
                {
                    var uri = CreateErrorUri("providerExists");
                    Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                    return Redirect(uri);
                }

                var userId = (await _userManager.FindByEmailAsync(externalLogin.Email)).Id;
                var info = await Authentication.GetExternalLoginInfoAsync();

                if (await _userManager.AddLoginAsync(userId, info.Login) == null)
                {
                    var uri = CreateErrorUri("error");
                    Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                    return Redirect(uri);
                }

                var identity = new ClaimsIdentity(externalLogin.GetClaims(), OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }
            else
            {
                var identity = new ClaimsIdentity(externalLogin.GetClaims(), OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        private async Task<IHttpActionResult> LoginAsync(ApplicationUser user, ExternalLoginData externalLogin, string clientId, bool hasLogin)
        {
            if (hasLogin)
            {
                await UpdateCookiesAndLoginAsync(user, externalLogin, clientId);
            }
            else
            {
                if (await _administrationService.UserEmailExistsAsync(externalLogin.Email) == false)
                {
                    var uri = CreateErrorUri("notFound");
                    Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                    return Redirect(uri);
                }

                if (await _administrationService.HasExistingExternalLoginAsync(externalLogin.Email, externalLogin.LoginProvider))
                {
                    var uri = CreateErrorUri("providerExists");
                    Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                    return Redirect(uri);
                }

                var identity = new ClaimsIdentity(externalLogin.GetClaims(), OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        private string CreateErrorUri(string tag)
        {
            var hostUri = Request.GetQueryNameValuePairs().First(e => e.Key == "redirect_uri").Value;
            var encodedError = Uri.EscapeDataString("Access_denied");
            return $"{hostUri}#{tag}={encodedError}";
        }

        private async Task UpdateCookiesAndLoginAsync(ApplicationUser user, ExternalLoginData externalLogin, string clientId)
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var oAuthIdentity = await _userManager.CreateIdentityAsync(user, OAuthDefaults.AuthenticationType);
            var cookieIdentity = await _userManager.CreateIdentityAsync(user, CookieAuthenticationDefaults.AuthenticationType);
            var properties = await CreateInitialRefreshToken(clientId, user, oAuthIdentity);

            if ((externalLogin.LoginProvider == "Google" && user.GoogleEmail == null) || (externalLogin.LoginProvider == "Facebook" && user.FacebookEmail == null) || (externalLogin.LoginProvider == "Microsoft" && user.MicrosoftEmail == null))
            {
                await _administrationService.AddProviderEmailAsync(user.Id, externalLogin.LoginProvider, externalLogin.Email);
            }

            Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
        }

        private void SetCookieExpirationDateToAccessTokenLifeTime(AuthenticationProperties properties)
        {
            // Set the .AspNet.Cookies. expiration date to the same date as the access token lifetime
            var lifeTimeHoursTimeSpan = TimeSpan.FromHours(Convert.ToInt16(_applicationSettings.AccessTokenLifeTimeInHours));
            var expirationDate = DateTime.UtcNow.Add(lifeTimeHoursTimeSpan);

            properties.ExpiresUtc = DateTime.SpecifyKind(expirationDate, DateTimeKind.Utc);
        }
    }
}
