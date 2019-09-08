using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Users;
using Shrooms.EntityModels.Models;

namespace Shrooms.Domain.Services.UserService
{
    public interface IUserService
    {
        Task ChangeUserLocalizationSettings(ChangeUserLocalizationSettingsDto settingsDto);

        Task ChangeUserNotificationSettings(UserNotificationsSettingsDto settingsDto, UserAndOrganizationDTO userOrg);

        void Delete(string userToDelete, UserAndOrganizationDTO userOrg);

        Task<LocalizationSettingsDto> GetUserLocalizationSettings(UserAndOrganizationDTO userOrg);

        IEnumerable<string> GetUserEmailsWithPermission(string permissionName, int orgId);

        IEnumerable<string> GetWallUserAppNotificationEnabledIds(string posterId, int wallId);

        IList<string> GetWallUsersEmails(string senderEmail, EntityModels.Models.Multiwall.Wall wall);

        Task<UserNotificationsSettingsDto> GetWallNotificationSettings(UserAndOrganizationDTO userOrg);

        void ChangeWallNotificationSettings(UserNotificationsSettingsDto wallNotificationsSettingsDto, UserAndOrganizationDTO userOrg);

        IEnumerable<IdentityUserLogin> GetUserLogins(string id);

        void RemoveLogin(string id, UserLoginInfo loginInfo);

        ApplicationUser GetApplicationUser(string id);
    }
}