using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Users;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Services.Picture;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.Services.UserService;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.Mocks;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class UserServiceTests
    {
        private const string SendUserEmail = "noreply@shrooms.com";

        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<WallMember> _wallUsersDbSet;
        private DbSet<WallModerator> _wallModeratorsDbSet;
        private DbSet<ApplicationRole> _rolesDbSet;
        private ShroomsUserManager _userManager;
        private IUserService _userService;

        [SetUp]
        public void Init()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = uow.MockDbSetForAsync(MockUsers());
            _rolesDbSet = uow.MockDbSetForAsync(MockRolesForMailing());
            _wallUsersDbSet = uow.MockDbSetForAsync<WallMember>();
            _wallModeratorsDbSet = uow.MockDbSetForAsync<WallModerator>();

            var dbContext = Substitute.For<IDbContext>();
            var userStore = Substitute.For<IUserStore<ApplicationUser>>();
            var pictureService = Substitute.For<IPictureService>();
            var roleService = Substitute.For<IRoleService>();
            _userManager = MockIdentity.MockUserManager(userStore, dbContext);

            _userService = new UserService(uow, _userManager, pictureService, roleService);
        }

        [Test]
        public void Should_Change_User_Settings()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", OrganizationId = 2, CultureCode = "en-US", TimeZone = "FLE Standard Time" }
            };
            _usersDbSet.SetDbSetDataForAsync(users);

            var changeCultureDto = new ChangeUserLocalizationSettingsDto { UserId = "user1", OrganizationId = 2, LanguageCode = "lt-LT", TimeZoneId = "Pacific Standard Time" };

            // Act
            _userService.ChangeUserLocalizationSettings(changeCultureDto);

            // Assert
            var result = _usersDbSet.First();
            Assert.AreEqual("lt-LT", result.CultureCode);
            Assert.AreEqual("Pacific Standard Time", result.TimeZone);
        }

        [Test]
        public void Should_Throw_If_Language_Is_Not_Supported()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", OrganizationId = 2, CultureCode = "en-US", TimeZone = "FLE Standard Time" }
            };
            _usersDbSet.SetDbSetDataForAsync(users);

            var changeSettingsDto = new ChangeUserLocalizationSettingsDto { UserId = "user1", OrganizationId = 2, LanguageCode = "en-GB", TimeZoneId = "FLE Standard Time" };

            // Act, Assert
            var result = _usersDbSet.First();
            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _userService.ChangeUserLocalizationSettings(changeSettingsDto));
            Assert.AreEqual(ErrorCodes.CultureUnsupported, ex.ErrorCode);
            Assert.AreEqual("en-US", result.CultureCode);
            Assert.AreEqual("FLE Standard Time", result.TimeZone);
        }

        [Test]
        public void Should_Throw_If_Timezone_Is_Not_Supported()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", OrganizationId = 2, CultureCode = "en-US", TimeZone = "FLE Standard Time" }
            };
            _usersDbSet.SetDbSetDataForAsync(users);

            var changeSettingsDto = new ChangeUserLocalizationSettingsDto { UserId = "user1", OrganizationId = 2, LanguageCode = "en-US", TimeZoneId = "unsupportedtz" };

            // Act, Assert
            var result = _usersDbSet.First();
            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _userService.ChangeUserLocalizationSettings(changeSettingsDto));
            Assert.AreEqual(ErrorCodes.TimezoneUnsupported, ex.ErrorCode);
            Assert.AreEqual("en-US", result.CultureCode);
            Assert.AreEqual("FLE Standard Time", result.TimeZone);
        }

        [Test]
        public async Task Should_Return_Supported_Languages_And_Timezones_With_User_Selected_In_First_Position()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", OrganizationId = 2, CultureCode = "en-US", TimeZone = "Pacific Standard Time" }
            };
            _usersDbSet.SetDbSetDataForAsync(users);

            var userOrg = new UserAndOrganizationDTO { UserId = "user1", OrganizationId = 2 };

            // Act
            var result = await _userService.GetUserLocalizationSettings(userOrg);

            // Assert
            Assert.IsInstanceOf<LocalizationSettingsDto>(result);
            Assert.AreEqual(CultureInfo.GetCultureInfo("en-US").DisplayName, result.Languages.First(x => x.IsSelected).DisplayName);
            Assert.AreEqual(TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time").DisplayName, result.TimeZones.First(x => x.IsSelected).DisplayName);
        }

        [Test]
        public void Should_Delete_User()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { OrganizationId = 2, Id = "userToDelete", RemainingKudos = 100, SpentKudos = 100, TotalKudos = 100 },
                new ApplicationUser { OrganizationId = 2, Id = "otherUser", RemainingKudos = 100, SpentKudos = 100, TotalKudos = 100 }
            };

            var userWalls = new List<WallMember>
            {
                new WallMember { WallId = 1, UserId = "userToDelete", Wall = new Wall { OrganizationId = 2 } },
                new WallMember { WallId = 2, UserId = "userToDelete", Wall = new Wall { OrganizationId = 2 } },
                new WallMember { WallId = 3, UserId = "userToDelete2", Wall = new Wall { OrganizationId = 2 } }
            };

            var moderators = new List<WallModerator>
            {
                new WallModerator { Id = 1, WallId = 1, UserId = "userToDelete", Wall = new Wall { OrganizationId = 2 } }
            };

            var taskSource = new TaskCompletionSource<ApplicationUser>();
            taskSource.SetResult(users.First());
            _userManager.FindByIdAsync("userToDelete").Returns(taskSource.Task);
            _usersDbSet.SetDbSetDataForAsync(users);
            _wallUsersDbSet.SetDbSetDataForAsync(userWalls);
            _wallModeratorsDbSet.SetDbSetDataForAsync(moderators);

            var userOrg = new UserAndOrganizationDTO
            {
                UserId = "admin",
                OrganizationId = 2
            };

            // Act
            _userService.DeleteAsync("userToDelete", userOrg);

            var deletedUser = _usersDbSet.First(x => x.Id == "userToDelete");

            // Assert
            Assert.AreEqual(deletedUser.RemainingKudos, 0);
            Assert.AreEqual(deletedUser.SpentKudos, 0);
            Assert.AreEqual(deletedUser.TotalKudos, 0);
            _usersDbSet.Received(1).Remove(Arg.Any<ApplicationUser>());
            _wallModeratorsDbSet.Received(1).Remove(Arg.Is<WallModerator>(m => m.UserId == "userToDelete"));
            _wallUsersDbSet.Received(2).Remove(Arg.Is<WallMember>(m => m.UserId == "userToDelete"));
        }

        [Test]
        public async Task Should_Return_User_Emails_Filterd_By_Permission()
        {
            MockRolesAndUsersForPermissionValidation();

            var userEmails = await _userService.GetUserEmailsWithPermissionAsync("TEST1_BASIC", 2);

            Assert.AreEqual(1, userEmails.Count);
            Assert.AreEqual("user1", userEmails.First());
        }

        [Test]
        public void Should_Change_User_Wall_Notification_Settings()
        {
            // Arrange
            MockUserWallNotifications();

            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 1,
                UserId = "UserId"
            };

            var notificationSettings = new List<WallNotificationsDto>
            {
                new WallNotificationsDto
                {
                    WallId = 1,
                    IsAppNotificationEnabled = false,
                    IsEmailNotificationEnabled = false
                },
                new WallNotificationsDto
                {
                    WallId = 3,
                    IsAppNotificationEnabled = true,
                    IsEmailNotificationEnabled = true
                },
                new WallNotificationsDto
                {
                    WallId = 4,
                    IsAppNotificationEnabled = false,
                    IsEmailNotificationEnabled = false
                }
            };

            var userSettings = new UserNotificationsSettingsDto
            {
                Walls = notificationSettings,
                EventsAppNotifications = true,
                EventsEmailNotifications = true,
                ProjectsAppNotifications = true,
                ProjectsEmailNotifications = true
            };

            // Act
            _userService.ChangeWallNotificationSettings(userSettings, userAndOrg);

            // Assert
            Assert.AreEqual(true, _wallUsersDbSet.First(x => x.WallId == 1).EmailNotificationsEnabled);
            Assert.AreEqual(true, _wallUsersDbSet.First(x => x.WallId == 3).EmailNotificationsEnabled);
            Assert.AreEqual(true, _wallUsersDbSet.First(x => x.WallId == 4).EmailNotificationsEnabled);
        }

        [Test]
        public async Task Should_Return_Wall_Members_Emails()
        {
            MockWallMembersForNotifications();

            var wall = new Wall
            {
                Id = 2,
                Type = WallType.UserCreated
            };

            var emails = await _userService.GetWallUsersEmailsAsync(SendUserEmail, wall);

            Assert.That(emails.Count, Is.EqualTo(1));
            Assert.That(emails.First().Contains("test3@shrooms.com"), Is.True);
        }

        #region Mocks

        private void MockRolesAndUsersForPermissionValidation()
        {
            var roles = new List<ApplicationRole>
            {
                new ApplicationRole
                {
                    Id = "roleId1",
                    OrganizationId = 2,
                    Permissions = new List<Permission>
                    {
                        new Permission
                        {
                            Name = "TEST1_BASIC"
                        }
                    }
                },
                new ApplicationRole
                {
                    Id = "roleId2",
                    OrganizationId = 2,
                    Permissions = new List<Permission>
                    {
                        new Permission
                        {
                            Name = "TEST1_ADMINISTRATION"
                        }
                    }
                },
                new ApplicationRole
                {
                    Id = "roleId3",
                    OrganizationId = 1,
                    Permissions = new List<Permission>
                    {
                        new Permission
                        {
                            Name = "TEST1_BASIC"
                        }
                    }
                }
            };

            _rolesDbSet.SetDbSetDataForAsync(roles.AsQueryable());

            var user1 = Substitute.For<ApplicationUser>();
            user1.Email = "user1";
            user1.Roles.Returns(
                new List<IdentityUserRole>
                {
                    new IdentityUserRole
                    {
                        RoleId = "roleId1"
                    }
                });

            var user2 = Substitute.For<ApplicationUser>();
            user2.Email = "user2";
            user2.Roles.Returns(
                new List<IdentityUserRole>
                {
                    new IdentityUserRole
                    {
                        RoleId = "roleId2"
                    }
                });

            var user3 = Substitute.For<ApplicationUser>();
            user3.Email = "user3";
            user3.Roles.Returns(
                new List<IdentityUserRole>
                {
                    new IdentityUserRole
                    {
                        RoleId = "roleId3"
                    }
                });

            var users = new List<ApplicationUser>
            {
                user1,
                user2,
                user3
            };

            _usersDbSet.SetDbSetDataForAsync(users);
        }

        private static IEnumerable<ApplicationRole> MockRolesForMailing()
        {
            var roles = new List<ApplicationRole>
            {
                new ApplicationRole
                {
                    Name = Roles.NewUser,
                    Id = "role1",
                    OrganizationId = 2
                },
                new ApplicationRole
                {
                    Name = Roles.External,
                    Id = "role2",
                    OrganizationId = 2
                }
            };

            return roles;
        }

        private IEnumerable<ApplicationUser> MockUsers()
        {
            return new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "1",
                    Email = "test1@shrooms.com",
                    WallUsers = new List<WallMember>
                    {
                        new WallMember
                        {
                            WallId = 1,
                            UserId = "1"
                        }
                    }
                },
                new ApplicationUser
                {
                    Id = "2",
                    Email = "test2@shrooms.com",
                    WallUsers = new List<WallMember>
                    {
                        new WallMember
                        {
                            WallId = 1,
                            UserId = "2"
                        }
                    }
                },
                new ApplicationUser
                {
                    Id = "3",
                    Email = "test3@shrooms.com",
                    WallUsers = new List<WallMember>
                    {
                        new WallMember
                        {
                            WallId = 1,
                            UserId = "3"
                        },
                        new WallMember
                        {
                            WallId = 2,
                            UserId = "3"
                        }
                    }
                }
            };
        }

        private void MockUserWallNotifications()
        {
            var wallUsers = new List<WallMember>
            {
                new WallMember
                {
                    UserId = "UserId",
                    EmailNotificationsEnabled = true,
                    WallId = 1,
                    Wall = new Wall
                    {
                        OrganizationId = 1,
                        Name = "MainWall",
                        Type = WallType.Main
                    }
                },
                new WallMember
                {
                    UserId = "UserId",
                    EmailNotificationsEnabled = true,
                    WallId = 2,
                    Wall = new Wall
                    {
                        OrganizationId = 1,
                        Name = "Wall1",
                        Type = WallType.UserCreated
                    }
                },
                new WallMember
                {
                    UserId = "UserId",
                    EmailNotificationsEnabled = false,
                    WallId = 3,
                    Wall = new Wall
                    {
                        OrganizationId = 1,
                        Name = "Wall2",
                        Type = WallType.UserCreated
                    }
                },
                new WallMember
                {
                    UserId = "UserId",
                    EmailNotificationsEnabled = true,
                    WallId = 4,
                    Wall = new Wall
                    {
                        OrganizationId = 1,
                        Name = "EventWall",
                        Type = WallType.Events
                    }
                }
            }.AsQueryable();

            _wallUsersDbSet.SetDbSetDataForAsync(wallUsers);
        }

        private void MockWallMembersForNotifications()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "1",
                    Email = "test1@shrooms.com",
                    WallUsers = new List<WallMember>
                    {
                        new WallMember
                        {
                            WallId = 1,
                            UserId = "1",
                            EmailNotificationsEnabled = true
                        }
                    }
                },
                new ApplicationUser
                {
                    Id = "2",
                    Email = "test2@shrooms.com",
                    WallUsers = new List<WallMember>
                    {
                        new WallMember
                        {
                            WallId = 1,
                            UserId = "2",
                            EmailNotificationsEnabled = true
                        },
                        new WallMember
                        {
                            WallId = 2,
                            UserId = "2",
                            EmailNotificationsEnabled = false
                        }
                    }
                },
                new ApplicationUser
                {
                    Id = "3",
                    Email = "test3@shrooms.com",
                    WallUsers = new List<WallMember>
                    {
                        new WallMember
                        {
                            WallId = 1,
                            UserId = "3",
                            EmailNotificationsEnabled = true
                        },
                        new WallMember
                        {
                            WallId = 2,
                            UserId = "3",
                            EmailNotificationsEnabled = true
                        }
                    }
                }
            }.AsQueryable();

            _usersDbSet.SetDbSetDataForAsync(users);
        }

        #endregion
    }
}
