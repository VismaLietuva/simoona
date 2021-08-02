using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Users;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.UserService
{
    public interface IUserService
    {
        Task ChangeUserLocalizationSettings(ChangeUserLocalizationSettingsDto settingsDto);

        Task ChangeUserNotificationSettings(UserNotificationsSettingsDto settingsDto, UserAndOrganizationDTO userOrg);

        Task Delete(string userToDelete, UserAndOrganizationDTO userOrg);

        Task<LocalizationSettingsDto> GetUserLocalizationSettings(UserAndOrganizationDTO userOrg);

        IEnumerable<string> GetUserEmailsWithPermission(string permissionName, int orgId);

        Task<IEnumerable<string>> GetWallUserAppNotificationEnabledIdsAsync(string posterId, int wallId);

        Task<IList<string>> GetWallUsersEmailsAsync(string senderEmail, DataLayer.EntityModels.Models.Multiwall.Wall wall);

        Task<UserNotificationsSettingsDto> GetWallNotificationSettings(UserAndOrganizationDTO userOrg);

        void ChangeWallNotificationSettings(UserNotificationsSettingsDto wallNotificationsSettingsDto, UserAndOrganizationDTO userOrg);

        IList<IdentityUserLogin> GetUserLogins(string id);

        void RemoveLogin(string id, UserLoginInfo loginInfo);

        Task<ApplicationUser> GetApplicationUserAsync(string id);

        Task<IEnumerable<ApplicationUser>> GetUsersWithMentionNotificationsAsync(IEnumerable<string> mentionedUsersIds);

        ApplicationUser GetApplicationUserOrDefault(string id);
        IEnumerable<UserAutoCompleteDto> GetUsersForAutocomplete(string s);
    }
}