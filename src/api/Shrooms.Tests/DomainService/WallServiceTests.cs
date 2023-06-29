using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.Services.Wall;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.ModelMappings;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class WallServiceTests
    {
        private DbSet<Wall> _wallsDbSet;
        private DbSet<WallModerator> _wallModeratorDbSet;
        private DbSet<WallMember> _wallUsersDbSet;
        private DbSet<ApplicationUser> _usersDbSet;
        private WallService _wallService;
        private IPermissionService _permissionService;
        private IUnitOfWork2 _uow;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();

            _wallsDbSet = _uow.MockDbSetForAsync<Wall>();
            _wallModeratorDbSet = _uow.MockDbSetForAsync<WallModerator>();
            _wallUsersDbSet = _uow.MockDbSetForAsync<WallMember>();
            _usersDbSet = _uow.MockDbSetForAsync<ApplicationUser>();

            _permissionService = Substitute.For<IPermissionService>();
            var roleService = Substitute.For<IRoleService>();

            MockRoleService(roleService);

            _uow.GetDbSet<WallModerator>().Returns(_wallModeratorDbSet);

            _wallService = new WallService(ModelMapper.Create(), _uow, _permissionService);
        }

        [Test]
        public async Task Moderator_Can_Update_Wall()
        {
            // Arrange
            var moderators = new List<WallModerator>
            {
                new()
                    { Id = 1, UserId = "user" }
            };
            var members = new List<WallMember>
            {
                new()
                    { Id = 1, UserId = "user1" }
            };
            var walls = new List<Wall>
            {
                new()
                    { Id = 1, OrganizationId = 2, Type = WallType.UserCreated, Name = "testname", Description = "testdesc", Logo = "testlogo", Moderators = moderators, Members = members }
            };

            var updateWallDto = new UpdateWallDto
            {
                Description = "desc",
                Logo = "logo",
                Name = "name",
                Id = 1,
                UserId = "user",
                OrganizationId = 2,
                ModeratorsIds = new[] { "1" }
            };

            const bool isWallAdministrator = false;
            _permissionService.UserHasPermissionAsync(updateWallDto, AdministrationPermissions.Wall).Returns(isWallAdministrator);
            _wallsDbSet.SetDbSetDataForAsync(walls.AsQueryable());

            // Act
            await _wallService.UpdateWallAsync(updateWallDto);

            // Assert
            var result = await _wallsDbSet.FirstAsync();
            Assert.AreEqual(updateWallDto.Name, result.Name);
            Assert.AreEqual(updateWallDto.Logo, result.Logo);
            Assert.AreEqual(updateWallDto.Description, result.Description);
            Assert.AreEqual(updateWallDto.Id, result.Id);
        }

        [Test]
        public async Task Administrator_Can_Update_Wall()
        {
            // Arrange
            var moderators = new List<WallModerator>
            {
                new()
                    { Id = 1, UserId = "user1" }
            };
            var members = new List<WallMember>
            {
                new()
                    { Id = 1, UserId = "user1" }
            };
            var walls = new List<Wall>
            {
                new()
                    { Id = 1, OrganizationId = 2, Type = WallType.UserCreated, Name = "testname", Description = "testdesc", Logo = "testlogo", Moderators = moderators, Members = members }
            };

            var updateWallDto = new UpdateWallDto
            {
                Description = "desc",
                Logo = "logo",
                Name = "name",
                Id = 1,
                UserId = "user",
                OrganizationId = 2,
                ModeratorsIds = new[] { "1" }
            };

            const bool isWallAdministrator = true;
            _permissionService.UserHasPermissionAsync(updateWallDto, AdministrationPermissions.Wall).Returns(isWallAdministrator);
            _wallsDbSet.SetDbSetDataForAsync(walls.AsQueryable());

            // Act
            await _wallService.UpdateWallAsync(updateWallDto);

            // Assert
            var result = await _wallsDbSet.FirstAsync();
            Assert.AreEqual(updateWallDto.Name, result.Name);
            Assert.AreEqual(updateWallDto.Logo, result.Logo);
            Assert.AreEqual(updateWallDto.Description, result.Description);
            Assert.AreEqual(updateWallDto.Id, result.Id);
        }

        [Test]
        public async Task Wall_Administrator_And_Moderator_Can_Update_Wall()
        {
            // Arrange
            var moderators = new List<WallModerator>
            {
                new()
                    { Id = 1, UserId = "user" }
            };
            var members = new List<WallMember>
            {
                new()
                    { Id = 1, UserId = "user1" }
            };
            var walls = new List<Wall>
            {
                new()
                    { Id = 1, OrganizationId = 2, Type = WallType.UserCreated, Name = "testname", Description = "testdesc", Logo = "testlogo", Moderators = moderators, Members = members }
            };

            var updateWallDto = new UpdateWallDto
            {
                Description = "desc",
                Logo = "logo",
                Name = "name",
                Id = 1,
                UserId = "user",
                OrganizationId = 2,
                ModeratorsIds = new[] { "1" }
            };

            const bool isWallAdministrator = true;
            _permissionService.UserHasPermissionAsync(updateWallDto, AdministrationPermissions.Wall).Returns(isWallAdministrator);
            _wallsDbSet.SetDbSetDataForAsync(walls.AsQueryable());

            // Act
            await _wallService.UpdateWallAsync(updateWallDto);

            // Assert
            var result = await _wallsDbSet.FirstAsync();
            Assert.AreEqual(updateWallDto.Name, result.Name);
            Assert.AreEqual(updateWallDto.Logo, result.Logo);
            Assert.AreEqual(updateWallDto.Description, result.Description);
            Assert.AreEqual(updateWallDto.Id, result.Id);
        }

        [Test]
        public void User_Can_Not_Update_Wall()
        {
            // Arrange
            var moderators = new List<WallModerator>
            {
                new()
                    { Id = 1, UserId = "user1" }
            };
            var walls = new List<Wall>
            {
                new()
                    { Id = 1, OrganizationId = 2, Type = WallType.UserCreated, Name = "testname", Description = "testdesc", Logo = "testlogo", Moderators = moderators }
            };

            var updateWallDto = new UpdateWallDto
            {
                Description = "desc",
                Logo = "logo",
                Name = "name",
                Id = 1,
                UserId = "user",
                OrganizationId = 2
            };

            const bool isWallAdministrator = false;
            _permissionService.UserHasPermissionAsync(updateWallDto, AdministrationPermissions.Wall).Returns(isWallAdministrator);
            _wallsDbSet.SetDbSetDataForAsync(walls.AsQueryable());

            // Act
            // Assert
            Assert.ThrowsAsync<UnauthorizedException>(async () => await _wallService.UpdateWallAsync(updateWallDto));
        }

        [Test]
        public void Throw_If_Wall_Does_Not_Exist_During_Update()
        {
            // Arrange
            var moderators = new List<WallModerator>
            {
                new()
                    { Id = 1, UserId = "user1" }
            };
            var walls = new List<Wall>
            {
                new()
                    { Id = 2, OrganizationId = 2, Type = WallType.UserCreated, Name = "testname", Description = "testdesc", Logo = "testlogo", Moderators = moderators }
            };

            var updateWallDto = new UpdateWallDto
            {
                Description = "desc",
                Logo = "logo",
                Name = "name",
                Id = 1,
                UserId = "user",
                OrganizationId = 2
            };

            const bool isWallAdministrator = false;
            _permissionService.UserHasPermissionAsync(updateWallDto, AdministrationPermissions.Wall).Returns(isWallAdministrator);
            _wallsDbSet.SetDbSetDataForAsync(walls.AsQueryable());

            // Act
            // Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _wallService.UpdateWallAsync(updateWallDto));
            Assert.AreEqual(ErrorCodes.ContentDoesNotExist, ex.ErrorCode);
        }

        [Test]
        public void Should_Throw_Validation_Exception_If_Wall_Name_Already_Exists_In_UserCreated_Walls()
        {
            // Arrange
            _wallsDbSet.SetDbSetDataForAsync(new List<Wall>
            {
                new()
                    { Id = 1, Name = "wall1", OrganizationId = 2, Type = WallType.UserCreated }
            }.AsQueryable());
            var newWallDto = new CreateWallDto
            {
                Name = "wall1",
                OrganizationId = 2,
                UserId = "wallCreator",
                Description = "wall1 desc"
            };

            // Act, Assert
            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _wallService.CreateNewWallAsync(newWallDto));
            Assert.AreEqual(ErrorCodes.WallNameAlreadyExists, ex.ErrorCode);
        }

        [Test]
        public async Task User_Can_Follow_Wall()
        {
            // Arrange
            var member = new WallMember { Id = 2, UserId = "user1", WallId = 2 };
            var jobPosition = new JobPosition { Title = "jobpos" };

            var walls = new List<Wall>
            {
                new()
                    { Id = 2, Members = new List<WallMember> { member }, OrganizationId = 1, Type = WallType.UserCreated, Moderators = new List<WallModerator>() }
            };

            var users = new List<ApplicationUser>
            {
                new()
                    { Id = "user2", FirstName = "fname", LastName = "lname", PictureId = "pic", OrganizationId = 1, JobPosition = jobPosition }
            };

            _usersDbSet.SetDbSetDataForAsync(users);
            _wallsDbSet.SetDbSetDataForAsync(walls);

            const int tenantId = 1;
            const string userId = "user2";

            // Act
            await _wallService.JoinOrLeaveWallAsync(2, userId, userId, tenantId, false);

            // Assert
            Assert.True((await _wallsDbSet.FirstAsync(x => x.Id == 2)).Members.Any(m => m.UserId == userId));
        }

        [Test]
        public void User_Can_Not_Follow_Different_Tenant_Wall()
        {
            MockWallsForJoinLeave();

            const int tenantId = 2;
            const string userId = "user2";

            Assert.ThrowsAsync<ValidationException>(async () => await _wallService.JoinOrLeaveWallAsync(2, userId, userId, tenantId, false));
        }

        [Test]
        public void User_Can_Not_Leave_Main_Wall()
        {
            MockWallsForJoinLeave();

            const int tenantId = 2;
            const string userId = "user1";

            Assert.ThrowsAsync<ValidationException>(async () => await _wallService.JoinOrLeaveWallAsync(1, userId, userId, tenantId, false));
        }

        [Test]
        public async Task User_Can_Leave_Event_Wall()
        {
            // Arrange
            var member4 = new WallMember { Id = 3, UserId = "user2", WallId = 4 };
            var jobPosition = new JobPosition { Title = "jobpos" };

            var walls = new List<Wall>
            {
                new()
                    { Id = 4, Members = new List<WallMember> { member4 }, OrganizationId = 2, Type = WallType.Events, Moderators = new List<WallModerator>() }
            };

            var users = new List<ApplicationUser>
            {
                new()
                    { Id = "user2", FirstName = "fname", LastName = "lname", PictureId = "pic", OrganizationId = 2, JobPosition = jobPosition }
            };

            _usersDbSet.SetDbSetDataForAsync(users);
            _wallsDbSet.SetDbSetDataForAsync(walls);

            const int tenantId = 2;
            const string userId = "user2";

            // Act
            await _wallService.JoinOrLeaveWallAsync(4, userId, userId, tenantId, true);

            // Assert
            _wallUsersDbSet.Received(1).Remove(Arg.Is<WallMember>(u => u.WallId == 4));
        }

        [Test]
        public void Wall_Moderator_Can_Not_Leave_Wall()
        {
            MockWallsForJoinLeave();

            const int tenantId = 2;
            const string userId = "user1";

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _wallService.JoinOrLeaveWallAsync(3, userId, userId, tenantId, false));
            Assert.AreEqual(ErrorCodes.WallModeratorCanNotLeave, ex.ErrorCode);
        }

        [Test]
        public async Task User_Can_Join_Event_Wall()
        {
            // Arrange
            var jobPosition = new JobPosition { Title = "jobpos" };
            var walls = new List<Wall>
            {
                new()
                    { Id = 4, Members = new List<WallMember>(), OrganizationId = 2, Type = WallType.Events, Moderators = new List<WallModerator>() }
            };

            var users = new List<ApplicationUser>
            {
                new()
                    { Id = "user3", FirstName = "fname", LastName = "lname", PictureId = "pic", OrganizationId = 2, JobPosition = jobPosition }
            };

            _usersDbSet.SetDbSetDataForAsync(users);
            _wallsDbSet.SetDbSetDataForAsync(walls);

            const int tenantId = 2;
            const string userId = "user3";

            // Act
            await _wallService.JoinOrLeaveWallAsync(4, userId, userId, tenantId, true);

            // Assert
            Assert.True((await _wallsDbSet.FirstAsync(w => w.Id == 4)).Members.Any(m => m.UserId == userId));
        }

        [Test]
        public async Task Wall_Moderator_Can_Add_User_To_Wall()
        {
            // Arrange
            const string moderatingUserId = "moderator1";
            const string attendingUserId = "user1";
            const int tenantId = 2;

            var jobPosition = new JobPosition { Title = "jobpos" };
            var users = new List<ApplicationUser>
            {
                new()
                    { Id = attendingUserId, OrganizationId = tenantId, JobPosition = jobPosition }
            };
            _usersDbSet.SetDbSetDataForAsync(users);

            var moderators = new List<WallModerator>
            {
                new()
                    { Id = 1, UserId = "moderator1", WallId = 1 }
            };

            var walls = new List<Wall>
            {
                new()
                    { Id = 1, OrganizationId = 2, Type = WallType.UserCreated, Moderators = moderators, Members = new List<WallMember>() }
            };
            _wallsDbSet.SetDbSetDataForAsync(walls);

            // Act
            await _wallService.JoinOrLeaveWallAsync(1, attendingUserId, moderatingUserId, tenantId, false);

            // Assert
            Assert.AreEqual(attendingUserId, walls.First().Members.First().UserId);
        }

        [Test]
        public async Task Wall_Moderator_Can_Remove_User_From_Wall()
        {
            // Arrange
            const string moderatingUserId = "moderator1";
            const string userToRemoveId = "user1";
            const int tenantId = 2;

            var jobPosition = new JobPosition { Title = "jobpos" };
            var users = new List<ApplicationUser>
            {
                new()
                    { Id = userToRemoveId, OrganizationId = tenantId, FirstName = "fname", LastName = "lname", PictureId = "pic", JobPosition = jobPosition }
            };
            _usersDbSet.SetDbSetDataForAsync(users);

            var moderators = new List<WallModerator>
            {
                new()
                    { Id = 1, UserId = "moderator1", WallId = 1 }
            };

            var members = new List<WallMember>
            {
                new()
                    { Id = 1, UserId = userToRemoveId, WallId = 1 }
            };

            var walls = new List<Wall>
            {
                new()
                    { Id = 1, OrganizationId = 2, Type = WallType.UserCreated, Moderators = moderators, Members = members }
            };
            _wallsDbSet.SetDbSetDataForAsync(walls);

            // Act
            await _wallService.JoinOrLeaveWallAsync(1, userToRemoveId, moderatingUserId, tenantId, false);

            // Assert
            _wallUsersDbSet.Received(1).Remove(Arg.Is<WallMember>(u => u.WallId == 1));
        }

        [Test]
        public void Wall_Moderator_Can_Not_Add_Non_Existent_User_To_Wall()
        {
            // Arrange
            const string moderatingUserId = "moderator1";
            const string attendingUserId = "user1";
            const int tenantId = 2;

            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser>());

            var moderators = new List<WallModerator>
            {
                new()
                    { Id = 1, UserId = "moderator1", WallId = 1 }
            };

            var walls = new List<Wall>
            {
                new()
                    { Id = 1, OrganizationId = 2, Type = WallType.UserCreated, Moderators = moderators, Members = new List<WallMember>() }
            };
            _wallsDbSet.SetDbSetDataForAsync(walls);

            // Act, Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _wallService.JoinOrLeaveWallAsync(1, attendingUserId, moderatingUserId, tenantId, false));
        }

        [Test]
        public void User_Can_Not_Add_Other_User_To_Wall()
        {
            // Arrange
            var moderators = new List<WallModerator>
            {
                new()
                    { Id = 1, UserId = "moderator1", WallId = 1 }
            };

            var walls = new List<Wall>
            {
                new()
                    { Id = 1, OrganizationId = 2, Type = WallType.UserCreated, Moderators = moderators, Members = new List<WallMember>() }
            };
            _wallsDbSet.SetDbSetDataForAsync(walls);

            const int tenantId = 2;
            const string attendingUserId = "user1";
            const string actingUserId = "user2";

            // Act, Assert
            Assert.ThrowsAsync<UnauthorizedException>(async () => await _wallService.JoinOrLeaveWallAsync(1, attendingUserId, actingUserId, tenantId, false));
        }

        [Test]
        public void Wall_User_Can_Not_Remove_User_From_Wall()
        {
            // Arrange
            const int tenantId = 2;
            const string userToRemoveId = "user1";
            const string actingUserId = "user2";

            var moderators = new List<WallModerator>
            {
                new()
                    { Id = 1, UserId = "moderator1", WallId = 1 }
            };

            var members = new List<WallMember>
            {
                new()
                    { Id = 1, UserId = userToRemoveId, WallId = 1 }
            };

            var walls = new List<Wall>
            {
                new()
                    { Id = 1, OrganizationId = 2, Type = WallType.UserCreated, Moderators = moderators, Members = members }
            };
            _wallsDbSet.SetDbSetDataForAsync(walls);

            // Act, Assert
            Assert.ThrowsAsync<UnauthorizedException>(async () => await _wallService.JoinOrLeaveWallAsync(1, userToRemoveId, actingUserId, tenantId, false));
        }

        [Test]
        public async Task Should_Add_Wall_Moderator_Successfully()
        {
            MockWallsForAddRemoveModerators();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "user2"
            };

            await _wallService.AddModeratorAsync(1, "user2", userOrg);
            _wallModeratorDbSet.Received(1).Add(Arg.Is<WallModerator>(x => x.UserId == "user2" && x.WallId == 1));
        }

        [Test]
        public async Task Should_Remove_Wall_Moderator_Successfully()
        {
            MockWallsForAddRemoveModerators();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "user2"
            };

            await _wallService.RemoveModeratorAsync(1, "user1", userOrg);
            _wallModeratorDbSet.Received(1).Remove(Arg.Is<WallModerator>(x => x.UserId == "user1" && x.WallId == 1));
        }

        [Test]
        public async Task Should_Return_Wall_Details_By_Id()
        {
            MockWallsForDetails();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "userId"
            };

            var wall = await _wallService.GetWallDetailsAsync(1, userOrg);

            Assert.AreEqual(1, wall.Id);
            Assert.AreEqual("Wall", wall.Name);
            Assert.AreEqual(true, wall.IsFollowing);
            Assert.AreEqual(1, wall.Moderators.Count());
            Assert.AreEqual("user1", wall.Moderators.First().Id);
            Assert.AreEqual("Description", wall.Description);
            Assert.AreEqual("Logo.jpg", wall.Logo);
            Assert.AreEqual(WallType.UserCreated, wall.Type);
        }

        [Test]
        public async Task Should_Return_Wall_Details_By_Id_2()
        {
            MockWallsForDetails();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "userId"
            };

            var wall = await _wallService.GetWallDetailsAsync(2, userOrg);

            Assert.AreEqual(2, wall.Id);
            Assert.AreEqual("Wall2", wall.Name);
            Assert.AreEqual(false, wall.IsFollowing);
            Assert.AreEqual(1, wall.Moderators.Count());
            Assert.AreEqual("user1", wall.Moderators.First().Id);
            Assert.AreEqual("Description2", wall.Description);
            Assert.AreEqual("Logo2.jpg", wall.Logo);
            Assert.AreEqual(WallType.UserCreated, wall.Type);
        }

        [Test]
        public async Task Should_Delete_Only_User_Created_Wall()
        {
            MockWallsForDelete();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "userId"
            };

            var wallToDelete = await _wallsDbSet.FirstAsync(x => x.Id == 2);
            await _wallService.DeleteWallAsync(2, userOrg, WallType.UserCreated);

            _wallsDbSet.Received(1).Remove(wallToDelete);
            await _uow.Received(1).SaveChangesAsync(userOrg.UserId);
        }

        [Test]
        public async Task Should_Delete_Only_Event_Wall()
        {
            MockWallsForDelete();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "userId"
            };

            var wallToDelete = await _wallsDbSet.FirstAsync(x => x.Id == 3);
            await _wallService.DeleteWallAsync(3, userOrg, WallType.Events);

            _wallsDbSet.Received(1).Remove(wallToDelete);
            await _uow.Received(1).SaveChangesAsync(userOrg.UserId);
        }

        [Test]
        public void Should_Throw_When_Deleting_Wall_With_Type_Other_Than_User_Created()
        {
            MockWallsForDelete();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "userId"
            };

            Assert.ThrowsAsync<ValidationException>(async () => await _wallService.DeleteWallAsync(1, userOrg, WallType.Events));
        }

        [Test]
        public void Should_Throw_When_Deleting_Wall_With_Type_Other_Than_Event()
        {
            MockWallsForDelete();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "userId"
            };

            Assert.ThrowsAsync<ValidationException>(async () => await _wallService.DeleteWallAsync(1, userOrg, WallType.UserCreated));
        }

        [Test]
        public void Should_Throw_When_User_Is_Not_Admin_Nor_Moderator()
        {
            MockWallsForDelete();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "userId2"
            };

            _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Wall).Returns(false);

            Assert.ThrowsAsync<UnauthorizedException>(async () => await _wallService.DeleteWallAsync(2, userOrg, WallType.UserCreated));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Throw_When_Not_Moderator_Tries_To_Modify_Wall_Content(bool checkForAdministrationEventPermission)
        {
            // Arrange
            const string createdBy = "other id";
            const string permission = "test";

            var userOrg = new UserAndOrganizationDto
            {
                UserId = "id",
                OrganizationId = 1
            };

            var wall = new Wall();

            _wallModeratorDbSet.SetDbSetDataForAsync(new List<WallModerator>());

            _permissionService.UserHasPermissionAsync(userOrg, Arg.Any<string>())
                .Returns(false);

            // Assert
            Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _wallService.CheckIfUserIsAllowedToModifyWallContentAsync(
                    wall,
                    createdBy,
                    permission,
                    userOrg,
                    checkForAdministrationEventPermission));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Not_Throw_When_Not_Moderator_Tries_To_Modify_Wall_Content(bool checkForAdministrationEventPermission)
        {
            // Arrange
            const string createdBy = "other id";
            const string permission = "test";
            const string userId = "id";
            const int wallId = 1;

            var userOrg = new UserAndOrganizationDto
            {
                UserId = userId,
                OrganizationId = 1
            };

            var wallModerator = new WallModerator
            {
                UserId = userId,
                WallId = wallId
            };

            var wall = new Wall
            {
                Id = wallId,
                Type = WallType.Events
            };

            _wallModeratorDbSet.SetDbSetDataForAsync(new List<WallModerator> { wallModerator });

            _permissionService.UserHasPermissionAsync(userOrg, Arg.Any<string>())
                .Returns(false);

            // Assert
            Assert.DoesNotThrowAsync(async () =>
                await _wallService.CheckIfUserIsAllowedToModifyWallContentAsync(
                    wall,
                    createdBy,
                    permission,
                    userOrg,
                    checkForAdministrationEventPermission));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Not_Throw_When_Owner_Tries_To_Modify_Wall_Content(bool checkForAdministrationEventPermission)
        {
            // Arrange
            const string permission = "test";
            const string userId = "id";
            const string createdBy = userId;
            const int wallId = 1;

            var userOrg = new UserAndOrganizationDto
            {
                UserId = userId,
                OrganizationId = 1
            };

            var wall = new Wall
            {
                Id = wallId,
                Type = WallType.Events
            };

            _wallModeratorDbSet.SetDbSetDataForAsync(new List<WallModerator>());

            _permissionService.UserHasPermissionAsync(userOrg, Arg.Any<string>())
                .Returns(false);

            // Assert
            Assert.DoesNotThrowAsync(async () =>
                await _wallService.CheckIfUserIsAllowedToModifyWallContentAsync(
                    wall,
                    createdBy,
                    permission,
                    userOrg,
                    checkForAdministrationEventPermission));
        }

        [TestCase(BasicPermissions.Post, true)]
        [TestCase(AdministrationPermissions.Post, false)]
        [TestCase(BasicPermissions.Post, false)]
        [TestCase(AdministrationPermissions.Post, true)]
        public void Should_Throw_When_Permission_Is_Not_Found(string permission, bool checkForAdministrationEventPermission)
        {
            // Arrange
            const string userId = "id";
            const string createdBy = "other id";
            const int wallId = 1;

            var userOrg = new UserAndOrganizationDto
            {
                UserId = userId,
                OrganizationId = 1
            };

            var wall = new Wall
            {
                Id = wallId,
                Type = WallType.Events
            };

            _wallModeratorDbSet.SetDbSetDataForAsync(new List<WallModerator>());

            _permissionService.UserHasPermissionAsync(userOrg, Arg.Any<string>())
                .Returns(false);

            // Assert
            Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _wallService.CheckIfUserIsAllowedToModifyWallContentAsync(
                    wall,
                    createdBy,
                    permission,
                    userOrg,
                    checkForAdministrationEventPermission));
        }

        [TestCase(BasicPermissions.Post, true)]
        [TestCase(AdministrationPermissions.Post, false)]
        [TestCase(BasicPermissions.Post, false)]
        [TestCase(AdministrationPermissions.Post, true)]
        public void Should_Not_Throw_When_Permission_Is_Found(string permission, bool checkForAdministrationEventPermission)
        {
            // Arrange
            const string userId = "id";
            const string createdBy = "other id";
            const int wallId = 1;

            var userOrg = new UserAndOrganizationDto
            {
                UserId = userId,
                OrganizationId = 1
            };

            var wall = new Wall
            {
                Id = wallId,
                Type = WallType.Events
            };

            _wallModeratorDbSet.SetDbSetDataForAsync(new List<WallModerator>());

            _permissionService.UserHasPermissionAsync(userOrg, Arg.Any<string>())
                .Returns(true);

            // Assert
            Assert.DoesNotThrowAsync(async () =>
                await _wallService.CheckIfUserIsAllowedToModifyWallContentAsync(
                    wall,
                    createdBy,
                    permission,
                    userOrg,
                    checkForAdministrationEventPermission));
        }

        private static void MockRoleService(IRoleService roleService)
        {
            var newRoleId = Guid.NewGuid().ToString();
            roleService.GetRoleIdByNameAsync(Roles.NewUser).Returns(newRoleId);
            roleService.ExcludeUsersWithRole(newRoleId).ReturnsForAnyArgs(x => true);
        }

        private void MockWallsForDelete()
        {
            var walls = new List<Wall>
            {
                new()
                {
                    Id = 1,
                    Type = WallType.Main,
                    OrganizationId = 2,
                    Moderators = new List<WallModerator>
                    {
                        new()
                        {
                            Id = 2,
                            UserId = "userId",
                            WallId = 2
                        }
                    }
                },
                new()
                {
                    Id = 2,
                    Type = WallType.UserCreated,
                    OrganizationId = 2,
                    Moderators = new List<WallModerator>
                    {
                        new()
                        {
                            Id = 2,
                            UserId = "userId",
                            WallId = 2
                        }
                    }
                },
                new()
                {
                    Id = 3,
                    Type = WallType.Events,
                    OrganizationId = 2,
                    Moderators = new List<WallModerator>
                    {
                        new()
                        {
                            Id = 3,
                            UserId = "userId",
                            WallId = 3
                        }
                    }
                }
            };

            _wallsDbSet.SetDbSetDataForAsync(walls.AsQueryable());
        }

        private void MockWallsForDetails()
        {
            var members = new List<WallMember>
            {
                new()
                    { UserId = "userId" }
            };
            var members1 = new List<WallMember>
            {
                new()
                    { Id = 2, UserId = "user1", WallId = 2 }
            };

            var moderators = new List<WallModerator>
            {
                new()
                    { Id = 2, UserId = "userId", WallId = 2 }
            };

            var walls = new List<Wall>
            {
                new()
                {
                    Id = 1,
                    Name = "Wall",
                    Type = WallType.UserCreated,
                    Description = "Description",
                    Logo = "Logo.jpg",
                    Members = members,
                    OrganizationId = 2
                },
                new()
                {
                    Id = 2,
                    Name = "Wall2",
                    Type = WallType.UserCreated,
                    Description = "Description2",
                    Logo = "Logo2.jpg",
                    Moderators = moderators,
                    Members = members1,
                    OrganizationId = 2
                }
            };

            var user = new ApplicationUser { Id = "user1", FirstName = "name", LastName = "surname" };

            var wallModerators = new List<WallModerator>
            {
                new()
                {
                    Id = 1,
                    UserId = "user1",
                    WallId = 1,
                    User = user
                },
                new()
                {
                    Id = 2,
                    UserId = "user1",
                    WallId = 2,
                    User = user
                }
            };

            _wallModeratorDbSet.SetDbSetDataForAsync(wallModerators);
            _wallsDbSet.SetDbSetDataForAsync(walls);
        }

        private void MockWallsForAddRemoveModerators()
        {
            var walls = new List<Wall>
            {
                new()
                {
                    Name = "Wall",
                    Id = 1,
                    Moderators = new List<WallModerator>
                    {
                        new()
                        {
                            Id = 1,
                            UserId = "user1",
                            WallId = 1
                        }
                    },
                    OrganizationId = 2
                }
            };

            _wallsDbSet.SetDbSetDataForAsync(walls.AsQueryable());
            _wallUsersDbSet.SetDbSetDataForAsync(new List<WallMember>().AsQueryable());
        }

        private void MockWallsForJoinLeave()
        {
            var member1 = new WallMember { Id = 1, UserId = "user1", WallId = 1 };
            var member2 = new WallMember { Id = 2, UserId = "user1", WallId = 2 };
            var member3 = new WallMember { Id = 3, UserId = "user1", WallId = 3 };
            var member4 = new WallMember { Id = 3, UserId = "user2", WallId = 4 };

            var walls = new List<Wall>
            {
                new()
                {
                    Name = "defaultWall",
                    Id = 1,
                    Members = new List<WallMember> { member1 },
                    OrganizationId = 2,
                    Type = WallType.Main,
                    Moderators = new List<WallModerator>()
                },
                new()
                {
                    Name = "wall1",
                    Id = 2,
                    Members = new List<WallMember> { member2 },
                    OrganizationId = 1,
                    Type = WallType.UserCreated,
                    Moderators = new List<WallModerator>()
                },
                new()
                {
                    Name = "wall2",
                    Id = 3,
                    Members = new List<WallMember> { member3 },
                    OrganizationId = 2,
                    Type = WallType.UserCreated,
                    Moderators = new List<WallModerator>
                    {
                        new()
                        {
                            UserId = "user1",
                            WallId = 3
                        }
                    }
                },
                new()
                {
                    Name = "EventWall",
                    Id = 4,
                    Members = new List<WallMember> { member4 },
                    OrganizationId = 2,
                    Type = WallType.Events,
                    Moderators = new List<WallModerator>()
                }
            };

            var users = new List<ApplicationUser>
            {
                new()
                    { Id = "user2", FirstName = "fname", LastName = "lname", PictureId = "pic", OrganizationId = 1 }
            };

            _usersDbSet.SetDbSetDataForAsync(users);
            _wallsDbSet.SetDbSetDataForAsync(walls);
        }
    }
}
