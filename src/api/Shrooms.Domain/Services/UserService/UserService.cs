﻿using System;
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
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.DataLayer.EntityModels.Models.Notifications;
using Shrooms.Domain.Services.Picture;
using Shrooms.Domain.Services.Roles;
using ConstantsRoles = Shrooms.Contracts.Constants.Roles;
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
        private readonly IRoleService _roleService;

        public UserService(IUnitOfWork2 uow, ShroomsUserManager userManager, IPictureService pictureService, IRoleService roleService)
        {
            _rolesDbSet = uow.GetDbSet<ApplicationRole>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _wallModeratorsDbSet = uow.GetDbSet<WallModerator>();
            _wallMembersDbSet = uow.GetDbSet<WallMember>();
            _wallDbSet = uow.GetDbSet<WallModel>();

            _uow = uow;
            _userManager = userManager;
            _pictureService = pictureService;
            _roleService = roleService;
        }

        public async Task ChangeUserLocalizationSettingsAsync(ChangeUserLocalizationSettingsDto settingsDto)
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

        public async Task ChangeUserNotificationSettingsAsync(UserNotificationsSettingsDto settingsDto, UserAndOrganizationDto userOrg)
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
            settings.MentionEmailNotifications = settingsDto.MentionEmailNotifications;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task<LocalizationSettingsDto> GetUserLocalizationSettingsAsync(UserAndOrganizationDto userOrg)
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

        public async Task DeleteAsync(string userToDelete, UserAndOrganizationDto userOrg)
        {
            var user = await _usersDbSet
                .SingleAsync(u => u.Id == userToDelete && u.OrganizationId == userOrg.OrganizationId);

            ClearUserKudos(user);
            await UnassignUserFromWallsAsync(userToDelete, userOrg.OrganizationId);
            await _userManager.RemoveLoginsAsync(userToDelete);

            _usersDbSet.Remove(user);

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task<IEnumerable<string>> GetWallUserAppNotificationEnabledIdsAsync(string posterId, int wallId)
        {
            var newUserAndExternalRoles = await _rolesDbSet
                .Where(r => r.Name == Contracts.Constants.Roles.NewUser ||
                            r.Name == Contracts.Constants.Roles.External)
                .ToListAsync();

            var newUserRoleId = newUserAndExternalRoles.First(r => r.Name == Contracts.Constants.Roles.NewUser).Id;
            var externalRoleId = newUserAndExternalRoles.First(r => r.Name == Contracts.Constants.Roles.External).Id;

            var wall = await _wallDbSet.SingleAsync(w => w.Id == wallId);

            var userAppNotificationEnabledIds = await _usersDbSet
                .Include(u => u.WallUsers)
                .Include(u => u.Roles)
                .Where(user => user.WallUsers.Any(x => x.WallId == wall.Id && x.AppNotificationsEnabled) &&
                               user.Roles.All(r => r.RoleId != newUserRoleId) &&
                               user.Id != posterId)
                .Where(ExternalRoleFilter(wall, externalRoleId))
                .Select(u => u.Id)
                .Distinct()
                .ToListAsync();

            return userAppNotificationEnabledIds;
        }

        public async Task<IEnumerable<UserAutoCompleteDto>> GetUsersForAutocompleteAsync(string s)
        {
            var newUserRoleId = await _roleService.GetRoleIdByNameAsync(ConstantsRoles.NewUser);

            var users = await _usersDbSet
                .Where(user => user.UserName.StartsWith(s) || user.Email.StartsWith(s) || (user.FirstName + " " + user.LastName).StartsWith(s) || user.LastName.StartsWith(s))
                .Where(_roleService.ExcludeUsersWithRole(newUserRoleId))
                .Take(10)
                .Select(MapUsersToAutocompleteDto())
                .ToListAsync();

            return users;
        }

        public async Task<IList<string>> GetWallUsersEmailsAsync(string senderEmail, WallModel wall)
        {
            var newUserAndExternalRoles = await _rolesDbSet
                .Where(r => r.Name == Contracts.Constants.Roles.NewUser ||
                            r.Name == Contracts.Constants.Roles.External)
                .ToListAsync();

            var newUserRoleId = newUserAndExternalRoles.First(r => r.Name == Contracts.Constants.Roles.NewUser).Id;
            var externalRoleId = newUserAndExternalRoles.First(r => r.Name == Contracts.Constants.Roles.External).Id;

            var emails = await _usersDbSet
                .Include(u => u.WallUsers)
                .Include(u => u.Roles)
                .Where(user => user.WallUsers.Any(x => x.WallId == wall.Id && x.EmailNotificationsEnabled) &&
                               user.Roles.All(r => r.RoleId != newUserRoleId) &&
                               user.Email != senderEmail)
                .Where(ExternalRoleFilter(wall, externalRoleId))
                .Select(u => u.Email)
                .Distinct()
                .ToListAsync();

            return emails;
        }

        public async Task<IList<string>> GetUserEmailsWithPermissionAsync(string permissionName, int orgId)
        {
            var rolesWithPermission = await _rolesDbSet
                .Include(x => x.Permissions)
                .Where(r => r.OrganizationId == orgId)
                .Where(r => r.Permissions.Any(x => x.Name == permissionName))
                .Select(role => role.Id)
                .ToListAsync();

            var userEmails = await _usersDbSet
                .Where(e => e.Roles.Any(x => rolesWithPermission.Contains(x.RoleId)))
                .Select(x => x.Email)
                .ToListAsync();

            return userEmails;
        }

        public async Task<UserNotificationsSettingsDto> GetWallNotificationSettingsAsync(UserAndOrganizationDto userOrg)
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
                MentionEmailNotifications = settings?.MentionEmailNotifications ?? true,

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

        public async Task ChangeWallNotificationSettingsAsync(UserNotificationsSettingsDto userNotificationsSettingsDto, UserAndOrganizationDto userOrg)
        {
            var wallIdsToUpdate = userNotificationsSettingsDto
                .Walls
                .Select(x => x.WallId)
                .ToList();

            var wallMembers = await _wallMembersDbSet
                .Include(x => x.Wall)
                .Where(x => x.UserId == userOrg.UserId &&
                            x.Wall.OrganizationId == userOrg.OrganizationId &&
                            wallIdsToUpdate.Contains(x.WallId) &&
                            x.Wall.Type == WallType.UserCreated)
                .ToListAsync();

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

            var eventOrProjectMembers = await _wallMembersDbSet
                .Include(x => x.Wall)
                .Where(x => x.UserId == userOrg.UserId &&
                            x.Wall.OrganizationId == userOrg.OrganizationId &&
                            (x.Wall.Type == WallType.Events ||
                             x.Wall.Type == WallType.Project))
                .ToListAsync();

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

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task<IList<IdentityUserLogin>> GetUserLoginsAsync(string id)
        {
            return (await _userManager.FindByIdAsync(id)).Logins.ToList();
        }

        public async Task RemoveLoginAsync(string id, UserLoginInfo loginInfo)
        {
            await _userManager.RemoveLoginAsync(id, loginInfo);

            var user = await _usersDbSet.FirstAsync(u => u.Id == id);

            if (loginInfo.LoginProvider == "Google")
            {
                user.GoogleEmail = null;
            }

            if (loginInfo.LoginProvider == "Facebook")
            {
                user.FacebookEmail = null;
            }

            await _uow.SaveChangesAsync(id);
        }

        public async Task<ApplicationUser> GetApplicationUserAsync(string id)
        {
            return await _usersDbSet.Include(x => x.NotificationsSettings).FirstAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersWithMentionNotificationsAsync(IEnumerable<string> ids)
        {
            return await _usersDbSet.Include(x => x.NotificationsSettings)
                .Where(u => (u.NotificationsSettings == null || u.NotificationsSettings.MentionEmailNotifications) && ids.Contains(u.Id))
                .ToListAsync();
        }

        public async Task<ApplicationUser> GetApplicationUserOrDefaultAsync(string id)
        {
            return await _usersDbSet.Include(x => x.NotificationsSettings).FirstOrDefaultAsync(u => u.Id == id);
        }

        private async Task UnassignUserFromWallsAsync(string userId, int tenantId)
        {
            var memberships = await _wallMembersDbSet
                .Include(m => m.Wall)
                .Where(m => m.Wall.OrganizationId == tenantId && m.UserId == userId)
                .ToListAsync();

            foreach (var membership in memberships)
            {
                _wallMembersDbSet.Remove(membership);
            }

            var moderatorMemberships = await _wallModeratorsDbSet
                .Include(m => m.Wall)
                .Where(m => m.Wall.OrganizationId == tenantId && m.UserId == userId)
                .ToListAsync();

            foreach (var moderator in moderatorMemberships)
            {
                _wallModeratorsDbSet.Remove(moderator);
            }
        }

        private static Expression<Func<ApplicationUser, UserAutoCompleteDto>> MapUsersToAutocompleteDto()
        {
            return u => new UserAutoCompleteDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FirstName + " " + u.LastName,
                UserName = u.UserName,
                PictureId = u.PictureId,
                Email = u.Email
            };
        }

        private static Expression<Func<ApplicationUser, bool>> ExternalRoleFilter(WallModel wall, string externalRoleId)
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

        private static void ClearUserKudos(ApplicationUser user)
        {
            user.RemainingKudos = 0;
            user.SpentKudos = 0;
            user.TotalKudos = 0;
        }
    }
}
