using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Users;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.DataLayer.EntityModels.Models.Notifications;
using Shrooms.Domain.Services.Picture;
using WallModel = Shrooms.DataLayer.EntityModels.Models.Multiwall.Wall;

namespace Shrooms.Domain.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IDbSet<ApplicationRole> _rolesDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<WallMember> _wallMembersDbSet;
        private readonly IDbSet<WallModerator> _wallModeratorsDbSet;
        private readonly IDbSet<WallModel> _wallDbSet;

        private readonly IUnitOfWork2 _uow;
        private readonly ShroomsUserManager _userManager;
        private readonly IPictureService _pictureService;

        public UserService(IUnitOfWork2 uow, ShroomsUserManager userManager, IPictureService pictureService)
        {
            _rolesDbSet = uow.GetDbSet<ApplicationRole>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _wallModeratorsDbSet = uow.GetDbSet<WallModerator>();
            _wallMembersDbSet = uow.GetDbSet<WallMember>();
            _wallDbSet = uow.GetDbSet<WallModel>();

            _uow = uow;
            _userManager = userManager;
            _pictureService = pictureService;
        }

        public async Task ChangeUserLocalizationSettings(ChangeUserLocalizationSettingsDto settingsDto)
        {
            var user = await _usersDbSet
                           .FirstAsync(u => u.Id == settingsDto.UserId && u.OrganizationId == settingsDto.OrganizationId);

            var culture = CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .FirstOrDefault(c => c.Name == settingsDto.LanguageCode);

            if (culture == null)
            {
                throw new ValidationException(ErrorCodes.CultureUnsupported, "Unsupported culture");
            }

            var isLanguageSupported = BusinessLayerConstants.SupportedLanguages.Any(x => x.LCID == culture.LCID);
            if (!isLanguageSupported)
            {
                throw new ValidationException(ErrorCodes.CultureUnsupported, "Unsupported culture");
            }

            var isTimezoneValid = TimeZoneInfo.GetSystemTimeZones().Any(tz => tz.Id == settingsDto.TimeZoneId);
            if (!isTimezoneValid)
            {
                throw new ValidationException(ErrorCodes.TimezoneUnsupported, "Unsupported timezone");
            }

            user.TimeZone = settingsDto.TimeZoneId;
            user.CultureCode = culture.Name;

            await _uow.SaveChangesAsync(settingsDto.UserId);
        }

        public async Task ChangeUserNotificationSettings(UserNotificationsSettingsDto settingsDto, UserAndOrganizationDTO userOrg)
        {
            var settings = await _usersDbSet
                               .Where(u => u.Id == userOrg.UserId && u.OrganizationId == userOrg.OrganizationId)
                               .Select(u => u.NotificationsSettings)
                               .FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new NotificationsSettings
                {
                    OrganizationId = userOrg.OrganizationId
                };
                var user = await _usersDbSet.FirstAsync(u => u.Id == userOrg.UserId && u.OrganizationId == userOrg.OrganizationId);
                user.NotificationsSettings = settings;
            }

            settings.ModifiedBy = userOrg.UserId;
            settings.EventsAppNotifications = settingsDto.EventsAppNotifications;
            settings.EventsEmailNotifications = settingsDto.EventsEmailNotifications;
            settings.EventWeeklyReminderAppNotifications = settingsDto.EventWeeklyReminderAppNotifications;
            settings.EventWeeklyReminderEmailNotifications = settingsDto.EventWeeklyReminderEmailNotifications;
            settings.ProjectsAppNotifications = settingsDto.ProjectsAppNotifications;
            settings.ProjectsEmailNotifications = settingsDto.ProjectsEmailNotifications;
            settings.MyPostsAppNotifications = settingsDto.MyPostsAppNotifications;
            settings.MyPostsEmailNotifications = settingsDto.MyPostsEmailNotifications;
            settings.FollowingPostsAppNotifications = settingsDto.FollowingPostsAppNotifications;
            settings.FollowingPostsEmailNotifications = settingsDto.FollowingPostsEmailNotifications;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task<LocalizationSettingsDto> GetUserLocalizationSettings(UserAndOrganizationDTO userOrg)
        {
            var userSettings = await _usersDbSet
                                   .Where(u => u.Id == userOrg.UserId && u.OrganizationId == userOrg.OrganizationId)
                                   .Select(u => new { u.CultureCode, u.TimeZone })
                                   .FirstAsync();

            var userCulture = CultureInfo.GetCultureInfo(userSettings.CultureCode);

            var settingsDto = new LocalizationSettingsDto
            {
                Languages = BusinessLayerConstants.SupportedLanguages
                    .Select(c => new LanguageDto { DisplayName = c.DisplayName, Name = c.Name, IsSelected = userCulture.LCID == c.LCID })
                    .OrderBy(l => l.DisplayName)
                    .ToList(),

                TimeZones = TimeZoneInfo.GetSystemTimeZones()
                    .Select(tz => new TimeZoneDto
                    {
                        Id = tz.Id,
                        DisplayName = tz.DisplayName,
                        IsSelected = string.Equals(tz.Id, userSettings.TimeZone, StringComparison.InvariantCultureIgnoreCase)
                    })
                    .ToList()
            };

            return settingsDto;
        }

        public async Task Delete(string userToDelete, UserAndOrganizationDTO userOrg)
        {
            var user = _usersDbSet
                .Single(u =>
                    u.Id == userToDelete &&
                    u.OrganizationId == userOrg.OrganizationId);

            ClearUserKudos(user);
            UnassignUserFromWalls(userToDelete, userOrg.OrganizationId);
            _userManager.RemoveLogins(userToDelete);

            await Anonymize(user, userOrg);

            _usersDbSet.Remove(user);
            _uow.SaveChanges(userOrg.UserId);
        }

        private async Task Anonymize(ApplicationUser user, UserAndOrganizationDTO userOrg)
        {
            await _pictureService.RemoveImage(user.PictureId, userOrg.OrganizationId);

            var randomString = Guid.NewGuid().ToString();
            user.Email = randomString;
            user.FirstName = randomString;
            user.LastName = randomString;
            user.PhoneNumber = randomString;
            user.UserName = randomString;
            user.FacebookEmail = randomString;
            user.GoogleEmail = randomString;
            user.Bio = string.Empty;
            user.PictureId = string.Empty;
            user.BirthDay = DateTime.UtcNow;

            _uow.SaveChanges(userOrg.UserId);
        }

        public IEnumerable<string> GetWallUserAppNotificationEnabledIds(string posterId, int wallId)
        {
            var newUserAndExternalRoles = _rolesDbSet
                .Where(r => r.Name == Contracts.Constants.Roles.NewUser ||
                            r.Name == Contracts.Constants.Roles.External)
                .ToList();

            var newUserRoleId = newUserAndExternalRoles.First(r => r.Name == Contracts.Constants.Roles.NewUser).Id;
            var externalRoleId = newUserAndExternalRoles.First(r => r.Name == Contracts.Constants.Roles.External).Id;

            var wall = _wallDbSet.Single(w => w.Id == wallId);

            var userAppNotificationEnabledIds = _usersDbSet
                .Include(u => u.WallUsers)
                .Include(u => u.Roles)
                .Where(user => user.WallUsers.Any(x => x.WallId == wall.Id && x.AppNotificationsEnabled) &&
                               user.Roles.All(r => r.RoleId != newUserRoleId) &&
                               user.Id != posterId)
                .Where(ExternalRoleFilter(wall, externalRoleId))
                .Select(u => u.Id)
                .Distinct()
                .ToList();

            return userAppNotificationEnabledIds;
        }

        public IList<string> GetWallUsersEmails(string senderEmail, WallModel wall)
        {
            var newUserAndExternalRoles = _rolesDbSet
                .Where(r => r.Name == Contracts.Constants.Roles.NewUser ||
                            r.Name == Contracts.Constants.Roles.External)
                .ToList();

            var newUserRoleId = newUserAndExternalRoles.First(r => r.Name == Contracts.Constants.Roles.NewUser).Id;
            var externalRoleId = newUserAndExternalRoles.First(r => r.Name == Contracts.Constants.Roles.External).Id;

            var emails = _usersDbSet
                .Include(u => u.WallUsers)
                .Include(u => u.Roles)
                .Where(user => user.WallUsers.Any(x => x.WallId == wall.Id && x.EmailNotificationsEnabled) &&
                               user.Roles.All(r => r.RoleId != newUserRoleId) &&
                               user.Email != senderEmail)
                .Where(ExternalRoleFilter(wall, externalRoleId))
                .Select(u => u.Email)
                .Distinct()
                .ToList();

            return emails;
        }

        public IEnumerable<string> GetUserEmailsWithPermission(string permissionName, int orgId)
        {
            var rolesWithPermission = _rolesDbSet
                .Include(x => x.Permissions)
                .Where(r => r.OrganizationId == orgId)
                .Where(r => r.Permissions.Any(x => x.Name == permissionName))
                .Select(role => role.Id)
                .ToList();

            var userEmails = _usersDbSet
                .Where(e => e.Roles.Any(x => rolesWithPermission.Contains(x.RoleId)))
                .Select(x => x.Email)
                .ToList();

            return userEmails;
        }

        public async Task<UserNotificationsSettingsDto> GetWallNotificationSettings(UserAndOrganizationDTO userOrg)
        {
            var settings = await _usersDbSet
                               .Where(u => u.Id == userOrg.UserId && u.OrganizationId == userOrg.OrganizationId)
                               .Select(u => u.NotificationsSettings)
                               .FirstOrDefaultAsync();

            var settingsDto = new UserNotificationsSettingsDto
            {
                EventsAppNotifications = settings?.EventsAppNotifications ?? true,
                EventsEmailNotifications = settings?.EventsEmailNotifications ?? true,
                EventWeeklyReminderAppNotifications = settings?.EventWeeklyReminderAppNotifications ?? true,
                EventWeeklyReminderEmailNotifications = settings?.EventWeeklyReminderEmailNotifications ?? true,
                ProjectsAppNotifications = settings?.ProjectsAppNotifications ?? true,
                ProjectsEmailNotifications = settings?.ProjectsEmailNotifications ?? true,
                MyPostsAppNotifications = settings?.MyPostsAppNotifications ?? true,
                MyPostsEmailNotifications = settings?.MyPostsEmailNotifications ?? true,
                FollowingPostsAppNotifications = settings?.FollowingPostsAppNotifications ?? true,
                FollowingPostsEmailNotifications = settings?.FollowingPostsEmailNotifications ?? true,

                Walls = _wallMembersDbSet
                    .Include(x => x.Wall)
                    .Where(x => x.UserId == userOrg.UserId && x.Wall != null && x.Wall.OrganizationId == userOrg.OrganizationId)
                    .Where(x => x.Wall.Type == WallType.UserCreated || x.Wall.Type == WallType.Main)
                    .Select(x => new WallNotificationsDto
                    {
                        WallName = x.Wall.Name,
                        WallId = x.WallId,
                        IsMainWall = x.Wall.Type == WallType.Main,
                        IsAppNotificationEnabled = x.AppNotificationsEnabled,
                        IsEmailNotificationEnabled = x.EmailNotificationsEnabled
                    })
            };

            return settingsDto;
        }

        public void ChangeWallNotificationSettings(UserNotificationsSettingsDto userNotificationsSettingsDto, UserAndOrganizationDTO userOrg)
        {
            var wallIdsToUpdate = userNotificationsSettingsDto
                .Walls
                .Select(x => x.WallId)
                .ToList();

            var wallMembers = _wallMembersDbSet
                .Include(x => x.Wall)
                .Where(x => x.UserId == userOrg.UserId &&
                            x.Wall.OrganizationId == userOrg.OrganizationId &&
                            wallIdsToUpdate.Contains(x.WallId) &&
                            x.Wall.Type == WallType.UserCreated)
                .ToList();

            foreach (var member in wallMembers)
            {
                member.EmailNotificationsEnabled = userNotificationsSettingsDto
                    .Walls
                    .First(x => x.WallId == member.WallId)
                    .IsEmailNotificationEnabled;

                member.AppNotificationsEnabled = userNotificationsSettingsDto
                    .Walls
                    .First(x => x.WallId == member.WallId)
                    .IsAppNotificationEnabled;
            }

            var eventOrProjectMembers = _wallMembersDbSet
                .Include(x => x.Wall)
                .Where(x => x.UserId == userOrg.UserId &&
                            x.Wall.OrganizationId == userOrg.OrganizationId &&
                            (x.Wall.Type == WallType.Events ||
                             x.Wall.Type == WallType.Project))
                .ToList();

            foreach (var member in eventOrProjectMembers)
            {
                switch (member.Wall.Type)
                {
                    case WallType.Events:
                        member.EmailNotificationsEnabled = userNotificationsSettingsDto.EventsEmailNotifications;
                        member.AppNotificationsEnabled = userNotificationsSettingsDto.EventsAppNotifications;
                        break;

                    case WallType.Project:
                        member.EmailNotificationsEnabled = userNotificationsSettingsDto.EventsEmailNotifications;
                        member.AppNotificationsEnabled = userNotificationsSettingsDto.EventsAppNotifications;
                        break;
                }
            }

            _uow.SaveChanges(userOrg.UserId);
        }

        public IList<IdentityUserLogin> GetUserLogins(string id)
        {
            return _userManager.FindById(id).Logins.ToList();
        }

        public void RemoveLogin(string id, UserLoginInfo loginInfo)
        {
            _userManager.RemoveLogin(id, loginInfo);

            var user = _usersDbSet.First(u => u.Id == id);
            if (loginInfo.LoginProvider == "Google")
            {
                user.GoogleEmail = null;
            }

            if (loginInfo.LoginProvider == "Facebook")
            {
                user.FacebookEmail = null;
            }

            _uow.SaveChanges(id);
        }

        public ApplicationUser GetApplicationUser(string id)
        {
            return _usersDbSet.First(u => u.Id == id);
        }

        private void UnassignUserFromWalls(string userId, int tenantId)
        {
            var memberships = _wallMembersDbSet
                .Include(m => m.Wall)
                .Where(m => m.Wall.OrganizationId == tenantId && m.UserId == userId)
                .ToList();

            foreach (var membership in memberships)
            {
                _wallMembersDbSet.Remove(membership);
            }

            var moderatorMemberships = _wallModeratorsDbSet
                .Include(m => m.Wall)
                .Where(m => m.Wall.OrganizationId == tenantId && m.UserId == userId)
                .ToList();

            foreach (var moderator in moderatorMemberships)
            {
                _wallModeratorsDbSet.Remove(moderator);
            }
        }

        private Expression<Func<ApplicationUser, bool>> ExternalRoleFilter(WallModel wall, string externalRoleId)
        {
            if (wall.Type != WallType.Events)
            {
                return user => user.Roles.All(r => r.RoleId != externalRoleId);
            }
            else
            {
                return user => true;
            }
        }

        private void ClearUserKudos(ApplicationUser user)
        {
            user.RemainingKudos = 0;
            user.SpentKudos = 0;
            user.TotalKudos = 0;
        }
    }
}