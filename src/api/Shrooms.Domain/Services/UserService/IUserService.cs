using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Users;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.DataLayer.EntityModels.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.UserService
{
    public interface IUserService
    {
        Task ChangeUserLocalizationSettingsAsync(ChangeUserLocalizationSettingsDto settingsDto);

        Task ChangeUserNotificationSettingsAsync(UserNotificationsSettingsDto settingsDto, UserAndOrganizationDto userOrg);

        Task DeleteAsync(string userToDelete, UserAndOrganizationDto userOrg);

        Task<LocalizationSettingsDto> GetUserLocalizationSettingsAsync(UserAndOrganizationDto userOrg);

        Task<IList<string>> GetUserEmailsWithPermissionAsync(string permissionName, int orgId);

        Task<IEnumerable<string>> GetWallUserAppNotificationEnabledIdsAsync(string posterId, int wallId);

        Task<IList<string>> GetWallUsersEmailsAsync(string senderEmail, DataLayer.EntityModels.Models.Multiwall.Wall wall);

        Task<UserNotificationsSettingsDto> GetWallNotificationSettingsAsync(UserAndOrganizationDto userOrg);

        Task ChangeWallNotificationSettingsAsync(UserNotificationsSettingsDto wallNotificationsSettingsDto, UserAndOrganizationDto userOrg);

        Task<IList<IdentityUserLogin>> GetUserLoginsAsync(string id);

        Task RemoveLoginAsync(string id, UserLoginInfo loginInfo);

        Task<ApplicationUser> GetApplicationUserAsync(string id);

        Task<IEnumerable<ApplicationUser>> GetUsersWithMentionNotificationsAsync(IEnumerable<string> mentionedUsersIds);

        Task<ApplicationUser> GetApplicationUserOrDefaultAsync(string id);
        Task<IEnumerable<UserAutoCompleteDto>> GetUsersForAutocompleteAsync(string s, bool includeSelf, UserAndOrganizationDto userOrg);
    }
}