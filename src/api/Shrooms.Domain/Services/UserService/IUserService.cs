using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Users;
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

        IEnumerable<string> GetWallUserAppNotificationEnabledIds(string posterId, int wallId);

        IList<string> GetWallUsersEmails(string senderEmail, DataLayer.EntityModels.Models.Multiwall.Wall wall);

        Task<UserNotificationsSettingsDto> GetWallNotificationSettings(UserAndOrganizationDTO userOrg);

        void ChangeWallNotificationSettings(UserNotificationsSettingsDto wallNotificationsSettingsDto, UserAndOrganizationDTO userOrg);

        IList<IdentityUserLogin> GetUserLogins(string id);

        void RemoveLogin(string id, UserLoginInfo loginInfo);

        ApplicationUser GetApplicationUser(string id);
    }
}