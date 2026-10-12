using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Users;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.UserPreferences;
using Shrooms.Domain.Services.UserService;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Controllers.Kudos;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.WebViewModels.Models.AccountModels;
using Shrooms.Presentation.WebViewModels.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [Route("User")]
    public class UserController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ISidebarFavoritesService _sidebarFavoritesService;

        public UserController(IMapper mapper, IUserService userService, ISidebarFavoritesService sidebarFavoritesService)
        {
            _mapper = mapper;
            _userService = userService;
            _sidebarFavoritesService = sidebarFavoritesService;
        }

        /// <summary>
        /// Returns the sidebar items the current user has starred.
        /// </summary>
        /// <response code="200">A JSON array of navigation item keys in display order, or null when never saved</response>
        [HttpGet]
        [Route("SidebarFavorites")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        [ProducesResponseType(typeof(SidebarFavoritesViewModel), StatusCodes.Status200OK)]
        public async Task<SidebarFavoritesViewModel> GetSidebarFavorites()
        {
            return new SidebarFavoritesViewModel
            {
                Favorites = await _sidebarFavoritesService.GetAsync(GetUserAndOrganization())
            };
        }

        /// <summary>
        /// Replaces the sidebar items the current user has starred.
        /// </summary>
        /// <param name="favorites">A JSON array of navigation item keys in display order</param>
        /// <response code="204">Favorites saved</response>
        /// <response code="400">Favorites are not a JSON array of strings, or are too long</response>
        [HttpPut]
        [Route("SidebarFavorites")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveSidebarFavorites([FromBody] SidebarFavoritesViewModel favorites)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _sidebarFavoritesService.SaveAsync(favorites.Favorites, GetUserAndOrganization());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return NoContent();
        }

        /// <summary>
        /// Returns supported languages and timezones with current users setting.
        /// </summary>
        /// <response code="200">Returns list of languages and timezones</response>
        /// <returns>List of languages and timezones</returns>
        [HttpGet]
        [Route("GeneralSettings")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        [ProducesResponseType(typeof(LocalizationSettingsViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLocalizationSettings()
        {
            var settingsDto = await _userService.GetUserLocalizationSettingsAsync(GetUserAndOrganization());
            var result = _mapper.Map<LocalizationSettingsDto, LocalizationSettingsViewModel>(settingsDto);
            return Ok(result);
        }

        /// <summary>
        /// Change user localization settings (Language, Timezone).
        /// </summary>
        /// <param name="localizationSettings">Pass languageCode and timeZoneId</param>
        /// <response code="200">Settings updated</response>
        /// <response code="400">Unsupported timezone/language</response>
        /// <returns>HTTP OK</returns>
        [HttpPut]
        [Route("GeneralSettings")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeLocalizationSettings(ChangeUserLocalizationSettingsViewModel localizationSettings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dto = _mapper.Map<ChangeUserLocalizationSettingsViewModel, ChangeUserLocalizationSettingsDto>(localizationSettings);
            SetOrganizationAndUser(dto);

            try
            {
                await _userService.ChangeUserLocalizationSettingsAsync(dto);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [Route("Notifications")]
        [PermissionAuthorize(BasicPermissions.ApplicationUser)]
        [ProducesResponseType(typeof(UserNotificationsSettingsViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWallNotifications()
        {
            var settings = await _userService.GetWallNotificationSettingsAsync(GetUserAndOrganization());
            var mappedsettings = _mapper.Map<UserNotificationsSettingsDto, UserNotificationsSettingsViewModel>(settings);
            return Ok(mappedsettings);
        }

        [HttpPut]
        [Route("Notifications")]
        [PermissionAuthorize(BasicPermissions.ApplicationUser)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeNotifications(UserNotificationsSettingsViewModel userNotificationsSettings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userNotificationsSettingsDto =
                _mapper.Map<UserNotificationsSettingsViewModel, UserNotificationsSettingsDto>(userNotificationsSettings);

            try
            {
                await _userService.ChangeUserNotificationSettingsAsync(userNotificationsSettingsDto, GetUserAndOrganization());
                await _userService.ChangeWallNotificationSettingsAsync(userNotificationsSettingsDto, GetUserAndOrganization());
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [Route("Logins")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        [ProducesResponseType(typeof(List<ProviderViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserLogins()
        {
            var id = GetUserAndOrganization().UserId;
            var user = await _userService.GetApplicationUserAsync(id);
            var logins = await _userService.GetUserLoginsAsync(id);
            if (logins == null)
            {
                return BadRequest();
            }

            var providers = new List<ProviderViewModel>();
            foreach (var login in logins)
            {
                providers.Add(new ProviderViewModel
                {
                    LoginProvider = login.LoginProvider,
                    Email = GetEmail(user, login.LoginProvider)
                });
            }

            return Ok(providers);
        }

        [HttpDelete]
        [Route("DeleteLogin")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LoginsUnlink(string providerName)
        {
            var userId = GetUserAndOrganization().UserId;
            var logins = await _userService.GetUserLoginsAsync(userId);

            if (logins.Count > 1)
            {
                try
                {
                    foreach (var login in logins.Where(l => l.LoginProvider == providerName))
                    {
                        await _userService.RemoveLoginAsync(userId, new Microsoft.AspNetCore.Identity.UserLoginInfo(login.LoginProvider, login.ProviderKey, login.LoginProvider));
                    }
                }
                catch (ArgumentException)
                {
                    return BadRequest();
                }
            }
            else
            {
                return BadRequest("Cannot remove the only provider");
            }

            return Ok();
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        [Route("GetUsersForAutocomplete")]
        public async Task<IEnumerable<ApplicationUserAutoCompleteViewModel>> GetUsersForAutocomplete(string s, bool includeSelf = true)
        {
            var userAutoCompleteDto = await _userService.GetUsersForAutocompleteAsync(s, includeSelf, GetUserAndOrganization());
            var result = _mapper.Map<IEnumerable<UserAutoCompleteDto>, IEnumerable<ApplicationUserAutoCompleteViewModel>>(userAutoCompleteDto);
            return result;
        }

        private static string GetEmail(ApplicationUser user, string provider)
        {
            if (provider == AuthenticationConstants.GoogleLoginProvider)
            {
                return user.GoogleEmail;
            }

            if (provider == AuthenticationConstants.FacebookLoginProvider)
            {
                return user.FacebookEmail;
            }

            if (provider == AuthenticationConstants.MicrosoftLoginProvider)
            {
                return user.MicrosoftEmail;
            }

            if (provider == AuthenticationConstants.InternalLoginProvider)
            {
                return user.UserName;
            }

            return null;
        }
    }
}
