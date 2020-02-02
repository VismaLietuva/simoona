using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Shrooms.API.Helpers;
using Shrooms.API.Providers;
using Shrooms.API.Results;
using Shrooms.Authentification.ExternalLoginInfrastructure;
using Shrooms.Authentification.Membership;
using Shrooms.Constants.Authentication;
using Shrooms.Constants.WebApi;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Domain.Services.Administration;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.RefreshTokens;
using Shrooms.EntityModels.Models;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.AccountModels;

namespace Shrooms.API.Controllers
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

        private IAuthenticationManager Authentication => Request.GetOwinContext().Authentication;

        private string RequestedOrganization => Request.GetRequestedTenant();

        public AccountController(IMapper mapper, ShroomsUserManager userManager, IPermissionService permissionService,
            IOrganizationService organizationService, IRefreshTokenService refreshTokenService, IAdministrationUsersService administrationService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _permissionService = permissionService;
            _organizationService = organizationService;
            _refreshTokenService = refreshTokenService;
            _administrationService = administrationService;
        }

        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public async Task<IHttpActionResult> GetUserInfo()
        {
            var externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
            if (externalLogin == null && User.Identity.IsAuthenticated)
            {
                var loggedUser = await GetLoggedInUserInfo();
                return Ok(loggedUser);
            }
            else
            {
                var externalUserInfo = new ExternalUserInfoViewModel
                {
                    Email = User.Identity.GetUserName(),
                    HasRegistered = externalLogin == null,
                    LoginProvider = externalLogin?.LoginProvider,
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

            if (_administrationService.UserEmailExists(model.Email))
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null && !user.EmailConfirmed && _administrationService.HasExistingExternalLogin(model.Email, AuthenticationConstants.InternalLoginProvider))
                {
                    await _userManager.RemovePasswordAsync(user.Id);
                    await _userManager.AddPasswordAsync(user.Id, model.Password);
                    await _administrationService.SendUserVerificationEmail(user, RequestedOrganization);

                    return Ok();
                }

                return BadRequest("User already exists");
            }

            if (_administrationService.UserIsSoftDeleted(model.Email))
            {
                _administrationService.RestoreUser(model.Email);

                return Ok();
            }

            var result = await _administrationService.CreateNewUser(_mapper.Map<ApplicationUser>(model), model.Password, RequestedOrganization);

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
            ClaimsIdentity oAuthIdentity = await _userManager.CreateIdentityAsync(user, OAuthDefaults.AuthenticationType);
            ClaimsIdentity cookieIdentity = await _userManager.CreateIdentityAsync(user, CookieAuthenticationDefaults.AuthenticationType);
            AuthenticationProperties properties = await CreateInitialRefreshToken(model.ClientId, user, oAuthIdentity);
            properties.IsPersistent = model.IsPersistance;

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

            await _administrationService.SendUserPasswordResetEmail(user, RequestedOrganization);

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
        public IHttpActionResult GetInternalLogins()
        {
            var logins = new List<ExternalLoginViewModel>();
            var organizationProviders = _organizationService
                .GetOrganizationByName(RequestedOrganization)
                .AuthenticationProviders;

            if (ContainsProvider(organizationProviders, AuthenticationConstants.InternalLoginProvider))
            {
                var internalLogin = new ExternalLoginViewModel
                {
                    Name = AuthenticationConstants.InternalLoginProvider
                };
                logins.Add(internalLogin);
            }

            return Ok(logins);
        }

        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userAndOrganization = GetUserAndOrganization();
                _refreshTokenService.RemoveTokenBySubject(userAndOrganization);
                _permissionService.RemoveCache(userAndOrganization.UserId);
                Authentication.SignOut();
            }

            return Ok();
        }

        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IHttpActionResult GetExternalLogins(string returnUrl, bool isLinkable = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
            var logins = new List<ExternalLoginViewModel>();
            var organizationProviders = _organizationService
                .GetOrganizationByName(RequestedOrganization)
                .AuthenticationProviders;

            foreach (AuthenticationDescription description in descriptions)
            {
                if (ContainsProvider(organizationProviders, description.Caption))
                {
                    var state = RandomOAuthStateGenerator.Generate(StateStrengthInBits);
                    ExternalLoginViewModel login = new ExternalLoginViewModel
                    {
                        Name = description.Caption,
                        Url = CreateUrl(description, returnUrl, state, isLinkable, false),
                        State = state
                    };
                    logins.Add(login);

                    state = RandomOAuthStateGenerator.Generate(StateStrengthInBits);
                    login = new ExternalLoginViewModel
                    {
                        Name = description.Caption + "Registration",
                        Url = CreateUrl(description, returnUrl, state, isLinkable, true),
                        State = state
                    };
                    logins.Add(login);
                }
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

            if (!_administrationService.UserEmailExists(info.Email))
            {
                if (_administrationService.UserIsSoftDeleted(info.Email))
                {
                    _administrationService.RestoreUser(info.Email);
                }
                else
                {
                    var requestedOrganization = RequestedOrganization;
                    var result = await _administrationService.CreateNewUserWithExternalLogin(info, requestedOrganization);
                    if (!result.Succeeded)
                    {
                        return GetErrorResult(result);
                    }
                }
            }
            else if (_administrationService.HasExistingExternalLogin(info.Email, info.Login.LoginProvider))
            {
                await _administrationService.AddProviderImage(_userManager.FindByEmail(info.Email).Id, info.ExternalIdentity);
                return Ok("User already exists");
            }
            else if (_administrationService.HasExistingExternalLogin(info.Email, AuthenticationConstants.InternalLoginProvider))
            {
                var user = await _userManager.FindByEmailAsync(info.Email);
                if (user != null && !user.EmailConfirmed)
                {
                    await _userManager.RemoveLoginAsync(user.Id, new UserLoginInfo(AuthenticationConstants.InternalLoginProvider, user.Id));
                    await _userManager.RemovePasswordAsync(user.Id);
                }
            }

            var userId = _userManager.FindByEmail(info.Email).Id;
            await _userManager.AddLoginAsync(userId, info.Login);
            await _administrationService.AddProviderImage(userId, info.ExternalIdentity);
            _administrationService.AddProviderEmail(userId, info.Login.LoginProvider, info.Email);

            return Ok();
        }

        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
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

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
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

            if (isRegistration == true && hasLogin == false)
            {
                var isEmailHostValid = _organizationService.IsOrganizationHostValid(externalLogin.Email, RequestedOrganization);
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
                return await LinkAccounts(externalLogin, userId);
            }

            // Registration process.
            if (isRegistration == true)
            {
                return await RegisterOrLogin(user, externalLogin, client_Id, hasLogin);
            }

            // Login process.
            return await Login(user, externalLogin, client_Id, hasLogin);
        }

        private bool ContainsProvider(string providerList, string providerName)
        {
            return providerList.ToLower().Contains(providerName.ToLower());
        }

        private async Task<AuthenticationProperties> CreateInitialRefreshToken(string client_Id, ApplicationUser user, ClaimsIdentity oAuthIdentity)
        {
            var userOrganization = new UserAndOrganizationDTO
            {
                OrganizationId = user.OrganizationId,
                UserId = user.Id
            };
            _refreshTokenService.RemoveTokenBySubject(userOrganization);

            AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.Id, client_Id);

            var ticket = new AuthenticationTicket(oAuthIdentity, properties);
            var context = new AuthenticationTokenCreateContext(Request.GetOwinContext(), Startup.OAuthServerOptions.RefreshTokenFormat, ticket);

            await Startup.OAuthServerOptions.RefreshTokenProvider.CreateAsync(context);
            properties.Dictionary.Add("refresh_token", context.Token);
            return properties;
        }

        private async Task<LoggedInUserInfoViewModel> GetLoggedInUserInfo()
        {
            var userId = User.Identity.GetUserId();
            var organizationId = User.Identity.GetOrganizationId();
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var user = await _userManager.FindByIdAsync(userId);
            var userInfo = new LoggedInUserInfoViewModel
            {
                HasRegistered = true,
                Roles = await _userManager.GetRolesAsync(userId),
                UserName = User.Identity.Name,
                UserId = userId,
                OrganizationName = claimsIdentity.FindFirstValue(WebApiConstants.ClaimOrganizationName),
                OrganizationId = claimsIdentity.FindFirstValue(WebApiConstants.ClaimOrganizationId),
                FullName = claimsIdentity.FindFirstValue(ClaimTypes.GivenName),
                Permissions = _permissionService.GetUserPermissions(userId, organizationId),
                Impersonated = claimsIdentity?.Claims.Any(c => c.Type == WebApiConstants.ClaimUserImpersonation && c.Value == true.ToString()) ?? false,
                CultureCode = user.CultureCode,
                TimeZone = user.TimeZone
            };
            return userInfo;
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
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

            return null;
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

        private async Task<IHttpActionResult> LinkAccounts(ExternalLoginData externalLogin, string userId)
        {
            var info = await Authentication.GetExternalLoginInfoAsync();
            if (await _userManager.AddLoginAsync(userId, info.Login) == null)
            {
                var uri = CreateErrorUri("error");
                return Redirect(uri);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            ClaimsIdentity identity = new ClaimsIdentity(externalLogin.GetClaims(), OAuthDefaults.AuthenticationType);
            Authentication.SignIn(identity);
            _administrationService.AddProviderEmail(userId, info.Login.LoginProvider, info.Email);
            return Ok();
        }

        private async Task<IHttpActionResult> RegisterOrLogin(ApplicationUser user, ExternalLoginData externalLogin, string client_Id, bool hasLogin)
        {
            if (hasLogin)
            {
                await UpdateCookiesAndLogin(user, externalLogin, client_Id);
            }
            else if (_administrationService.UserEmailExists(externalLogin.Email))
            {
                if (_administrationService.HasExistingExternalLogin(externalLogin.Email, externalLogin.LoginProvider))
                {
                    var uri = CreateErrorUri("providerExists");
                    Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                    return Redirect(uri);
                }

                var userId = _userManager.FindByEmail(externalLogin.Email).Id;
                var info = await Authentication.GetExternalLoginInfoAsync();
                if (await _userManager.AddLoginAsync(userId, info.Login) == null)
                {
                    var uri = CreateErrorUri("error");
                    Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                    return Redirect(uri);
                }

                ClaimsIdentity identity = new ClaimsIdentity(externalLogin.GetClaims(), OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }
            else
            {
                ClaimsIdentity identity = new ClaimsIdentity(externalLogin.GetClaims(), OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        private async Task<IHttpActionResult> Login(ApplicationUser user, ExternalLoginData externalLogin, string client_Id, bool hasLogin)
        {
            if (hasLogin)
            {
                await UpdateCookiesAndLogin(user, externalLogin, client_Id);
            }
            else
            {
                if (_administrationService.UserEmailExists(externalLogin.Email) == false)
                {
                    var uri = CreateErrorUri("notFound");
                    Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                    return Redirect(uri);
                }

                if (_administrationService.HasExistingExternalLogin(externalLogin.Email, externalLogin.LoginProvider))
                {
                    var uri = CreateErrorUri("providerExists");
                    Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                    return Redirect(uri);
                }

                ClaimsIdentity identity = new ClaimsIdentity(externalLogin.GetClaims(), OAuthDefaults.AuthenticationType);
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

        private async Task UpdateCookiesAndLogin(ApplicationUser user, ExternalLoginData externalLogin, string client_Id)
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            ClaimsIdentity oAuthIdentity = await _userManager.CreateIdentityAsync(user, OAuthDefaults.AuthenticationType);
            ClaimsIdentity cookieIdentity = await _userManager.CreateIdentityAsync(user, CookieAuthenticationDefaults.AuthenticationType);
            AuthenticationProperties properties = await CreateInitialRefreshToken(client_Id, user, oAuthIdentity);
            if ((externalLogin.LoginProvider == "Google" && user.GoogleEmail == null) || (externalLogin.LoginProvider == "Facebook" && user.FacebookEmail == null))
            {
                _administrationService.AddProviderEmail(user.Id, externalLogin.LoginProvider, externalLogin.Email);
            }

            Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
        }
    }
}