using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class RoleServiceTest
    {
        private IRoleService _roleService;
        private IPermissionService _permissionService;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<ApplicationRole> _roleDbSet;
        private DbSet<IdentityUserRole<string>> _userRolesDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IAsyncEnumerable<ApplicationUser>>();
            _roleDbSet = Substitute.For<DbSet<ApplicationRole>, IQueryable<ApplicationRole>, IAsyncEnumerable<ApplicationRole>>();
            _userRolesDbSet = Substitute.For<DbSet<IdentityUserRole<string>>, IQueryable<IdentityUserRole<string>>, IAsyncEnumerable<IdentityUserRole<string>>>();

            uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);
            uow.GetDbSet<ApplicationRole>().Returns(_roleDbSet);
            uow.GetDbSet<IdentityUserRole<string>>().Returns(_userRolesDbSet);

            _permissionService = Substitute.For<IPermissionService>();

            _roleService = new RoleService(uow, _permissionService);
        }

        [Test]
        public async Task OnlyUsersWithRoleName_KeepsOnlyMembersOfThatRole()
        {
            var options = new DbContextOptionsBuilder<ShroomsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            await using var dbContext = new ShroomsDbContext(options);
            dbContext.Roles.AddRange(
                new ApplicationRole { Id = "manager-role", Name = "Manager" },
                new ApplicationRole { Id = "user-role", Name = "User" });
            dbContext.Users.AddRange(
                new ApplicationUser { Id = "manager", UserName = "manager" },
                new ApplicationUser { Id = "employee", UserName = "employee" });
            dbContext.UserRoles.AddRange(
                new IdentityUserRole<string> { UserId = "manager", RoleId = "manager-role" },
                new IdentityUserRole<string> { UserId = "employee", RoleId = "user-role" });
            await dbContext.SaveChangesAsync(false);
            var roleService = new RoleService(new UnitOfWork2(dbContext), _permissionService);

            var userIds = await dbContext.Users
                .Where(roleService.OnlyUsersWithRoleName("Manager"))
                .Select(user => user.Id)
                .ToListAsync();

            Assert.That(userIds, Is.EqualTo(new[] { "manager" }));
        }

        [Test]
        public void GetRoleIdByNameAsync_CancelledToken_StopsTheQuery()
        {
            var options = new DbContextOptionsBuilder<ShroomsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var dbContext = new ShroomsDbContext(options);
            var roleService = new RoleService(new UnitOfWork2(dbContext), _permissionService);

            Assert.That(
                async () => await roleService.GetRoleIdByNameAsync("NewUser", new CancellationToken(true)),
                Throws.InstanceOf<OperationCanceledException>());
        }

        [Test]
        public async Task Should_Get_Correctly_Mapped_Roles_For_AutoComplete()
        {
            MockRolesForAutocomplete();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            var roles = (await _roleService.GetRolesForAutocompleteAsync("Test1", userOrg)).ToList();

            Assert.That(roles.Count, Is.EqualTo(2));
            Assert.That(roles.ToArray()[0].Id, Is.EqualTo("roleId1"));
            Assert.That(roles.ToArray()[1].Id, Is.EqualTo("roleId3"));
        }

        [Test]
        public async Task Should_Get_Role_With_All_Permissions_And_Users()
        {
            MockRoles();

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var roles = await _roleService.GetRoleByIdAsync(userAndOrg, "roleId1");

            Assert.That(roles.Name, Is.EqualTo("Test1"));
            Assert.That(roles.Permissions.Count(), Is.EqualTo(3));
            Assert.That(roles.Permissions.ToArray()[0].ActiveScope, Is.EqualTo(PermissionScopes.Basic));
            Assert.That(roles.Permissions.ToArray()[1].ActiveScope, Is.EqualTo(PermissionScopes.Administration));
            Assert.That(roles.Permissions.ToArray()[2].ActiveScope, Is.EqualTo(""));
            Assert.That(roles.Users.Count(), Is.EqualTo(2));
            Assert.That(roles.Users.ToArray()[0].FullName, Is.EqualTo("first1 last1"));
            Assert.That(roles.Users.ToArray()[1].FullName, Is.EqualTo("first2 last2"));
        }

        [Test]
        public async Task GetRoleByIdAsync_RoleWithOnlyKudosShopAdministration_ReportsOnlyKudosShop()
        {
            var (roleService, _) = await CreateRoleEditorAsync(AdministrationPermissions.KudosShop);

            var role = await roleService.GetRoleByIdAsync(EditorOrganization, EditedRoleId);

            var scopes = role.Permissions.ToDictionary(group => group.Name, group => group.ActiveScope);
            Assert.That(scopes["kudosshop"], Is.EqualTo(PermissionScopes.Administration));
            Assert.That(scopes["kudos"], Is.Empty);
            Assert.That(scopes["kudosbasket"], Is.Empty);
        }

        [TestCase(BasicPermissions.EventUsers, "eventusers", "event")]
        [TestCase(BasicPermissions.OfficeUsers, "officeusers", "office")]
        [TestCase(BasicPermissions.OrganizationalStructure, "organizationalstructure", "organization")]
        [TestCase(AdministrationPermissions.RoomType, "roomtype", "room")]
        [TestCase(BasicPermissions.KudosBasket, "kudosbasket", "kudos")]
        public async Task GetRoleByIdAsync_PermissionWhoseGroupStartsLikeAnother_ReportsOnlyItsOwnGroup(string permission, string ownGroup, string otherGroup)
        {
            var (roleService, _) = await CreateRoleEditorAsync(permission);

            var role = await roleService.GetRoleByIdAsync(EditorOrganization, EditedRoleId);

            var scopes = role.Permissions.ToDictionary(group => group.Name, group => group.ActiveScope);
            Assert.That(scopes[ownGroup], Is.Not.Empty);
            Assert.That(scopes[otherGroup], Is.Empty);
        }

        [TestCaseSource(nameof(RoundTripRoles))]
        public async Task GetPermissionIdsToAssignAsync_SavingWhatGetReturned_KeepsTheRolePermissions(string[] rolePermissions)
        {
            var (roleService, dbContext) = await CreateRoleEditorAsync(rolePermissions);
            var role = await roleService.GetRoleByIdAsync(EditorOrganization, EditedRoleId);

            var savedIds = await roleService.GetPermissionIdsToAssignAsync(EditorOrganization.OrganizationId, await GetRolePermissionIdsAsync(dbContext), role.Permissions);

            Assert.That(await GetPermissionNamesAsync(dbContext, savedIds), Is.EquivalentTo(rolePermissions));
        }

        [Test]
        public async Task GetPermissionIdsToAssignAsync_GroupSetToAdmin_GrantsTheWholeGroupAndNothingFromSimilarlyNamedGroups()
        {
            var (roleService, dbContext) = await CreateRoleEditorAsync();
            var role = await roleService.GetRoleByIdAsync(EditorOrganization, EditedRoleId);
            role.Permissions.Single(group => group.Name == "kudos").ActiveScope = PermissionScopes.Administration;

            var savedIds = await roleService.GetPermissionIdsToAssignAsync(EditorOrganization.OrganizationId, Array.Empty<int>(), role.Permissions);

            Assert.That(await GetPermissionNamesAsync(dbContext, savedIds), Is.EquivalentTo(new[] { BasicPermissions.Kudos, AdministrationPermissions.Kudos }));
        }

        [Test]
        public async Task GetPermissionIdsToAssignAsync_GroupSetToBasic_GrantsOnlyTheGroupBasicPermissions()
        {
            var (roleService, dbContext) = await CreateRoleEditorAsync();
            var role = await roleService.GetRoleByIdAsync(EditorOrganization, EditedRoleId);
            role.Permissions.Single(group => group.Name == "event").ActiveScope = PermissionScopes.Basic;

            var savedIds = await roleService.GetPermissionIdsToAssignAsync(EditorOrganization.OrganizationId, Array.Empty<int>(), role.Permissions);

            Assert.That(await GetPermissionNamesAsync(dbContext, savedIds), Is.EquivalentTo(new[] { BasicPermissions.Event }));
        }

        [Test]
        public async Task GetPermissionIdsToAssignAsync_AdminOnlyGroupChangedToBasicAndBack_AdminImpliesBasic()
        {
            var (roleService, dbContext) = await CreateRoleEditorAsync(AdministrationPermissions.Kudos);
            var kudos = new PermissionGroupDto { Name = "kudos", ActiveScope = PermissionScopes.Basic };

            var basicIds = await roleService.GetPermissionIdsToAssignAsync(EditorOrganization.OrganizationId, await GetRolePermissionIdsAsync(dbContext), new[] { kudos });
            kudos.ActiveScope = PermissionScopes.Administration;
            var adminIds = await roleService.GetPermissionIdsToAssignAsync(EditorOrganization.OrganizationId, basicIds, new[] { kudos });

            Assert.That(await GetPermissionNamesAsync(dbContext, basicIds), Is.EquivalentTo(new[] { BasicPermissions.Kudos }));
            Assert.That(await GetPermissionNamesAsync(dbContext, adminIds), Is.EquivalentTo(new[] { BasicPermissions.Kudos, AdministrationPermissions.Kudos }));
        }

        [Test]
        public async Task GetPermissionIdsToAssignAsync_GroupLeftOut_RemovesItsPermissions()
        {
            var (roleService, dbContext) = await CreateRoleEditorAsync(BasicPermissions.Kudos, AdministrationPermissions.KudosShop);
            var role = await roleService.GetRoleByIdAsync(EditorOrganization, EditedRoleId);

            var savedIds = await roleService.GetPermissionIdsToAssignAsync(
                EditorOrganization.OrganizationId,
                await GetRolePermissionIdsAsync(dbContext),
                role.Permissions.Where(group => group.Name != "kudosshop"));

            Assert.That(await GetPermissionNamesAsync(dbContext, savedIds), Is.EquivalentTo(new[] { BasicPermissions.Kudos }));
        }

        private static readonly UserAndOrganizationDto EditorOrganization = new() { OrganizationId = 1, UserId = "editor" };

        private const string EditedRoleId = "edited-role";

        private const string PollAdministrationWithLongScope = AdministrationPermissions.Poll;

        private const string DisabledModulePermission = "LOTTERY_BASIC";

        private static IEnumerable<TestCaseData> RoundTripRoles()
        {
            yield return new TestCaseData((object)Array.Empty<string>()).SetName("RoundTrip_NoPermissions");
            yield return new TestCaseData((object)new[] { AdministrationPermissions.KudosShop }).SetName("RoundTrip_OnlyKudosShopAdministration");
            yield return new TestCaseData((object)new[] { BasicPermissions.KudosBasket, AdministrationPermissions.KudosBasket }).SetName("RoundTrip_KudosBasketWithoutKudos");
            yield return new TestCaseData((object)new[] { BasicPermissions.EventUsers, BasicPermissions.OfficeUsers, BasicPermissions.OrganizationalStructure, AdministrationPermissions.RoomType })
                .SetName("RoundTrip_GroupsThatStartLikeOtherGroups");
            yield return new TestCaseData((object)new[] { AdministrationPermissions.Kudos, AdministrationPermissions.Event }).SetName("RoundTrip_AdministrationWithoutBasic");
            yield return new TestCaseData((object)new[] { BasicPermissions.Poll, PollAdministrationWithLongScope }).SetName("RoundTrip_PollAdministrationStoredWithLongScopeName");
            yield return new TestCaseData((object)new[] { BasicPermissions.Kudos, DisabledModulePermission }).SetName("RoundTrip_PermissionOfModuleTheOrganizationDoesNotUse");
            yield return new TestCaseData((object)EditorPermissions.Select(permission => permission.Name).ToArray()).SetName("RoundTrip_EveryPermission");
        }

        private static readonly (string Name, string Scope)[] EditorPermissions =
        {
            (BasicPermissions.Kudos, PermissionScopes.Basic),
            (AdministrationPermissions.Kudos, PermissionScopes.Administration),
            (BasicPermissions.KudosBasket, PermissionScopes.Basic),
            (AdministrationPermissions.KudosBasket, PermissionScopes.Administration),
            (AdministrationPermissions.KudosShop, PermissionScopes.Administration),
            (BasicPermissions.Event, PermissionScopes.Basic),
            (AdministrationPermissions.Event, PermissionScopes.Administration),
            (BasicPermissions.EventUsers, PermissionScopes.Basic),
            (BasicPermissions.Office, PermissionScopes.Basic),
            (AdministrationPermissions.Office, PermissionScopes.Administration),
            (BasicPermissions.OfficeUsers, PermissionScopes.Basic),
            (BasicPermissions.Organization, PermissionScopes.Basic),
            (AdministrationPermissions.Organization, PermissionScopes.Administration),
            (BasicPermissions.OrganizationalStructure, PermissionScopes.Basic),
            (BasicPermissions.Room, PermissionScopes.Basic),
            (AdministrationPermissions.Room, PermissionScopes.Administration),
            (AdministrationPermissions.RoomType, PermissionScopes.Administration),
            (BasicPermissions.Poll, PermissionScopes.Basic),
            // The AddPolls migration stored this one with the scope "administration" rather than "admin".
            (PollAdministrationWithLongScope, "administration"),
            (DisabledModulePermission, PermissionScopes.Basic)
        };

        /// <summary>
        /// An in-memory database with <see cref="EditorPermissions"/> and a role holding <paramref name="rolePermissions"/>,
        /// read through the real <see cref="PermissionService"/> so the test sees the groups the editor sees.
        /// <see cref="DisabledModulePermission"/> belongs to a module the organization does not use, so the editor does not show it.
        /// </summary>
        private async Task<(RoleService RoleService, ShroomsDbContext DbContext)> CreateRoleEditorAsync(params string[] rolePermissions)
        {
            var options = new DbContextOptionsBuilder<ShroomsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var dbContext = new ShroomsDbContext(options);

            var disabledModule = new Module { Id = 1, Name = "Lottery", Organizations = new List<Organization>() };
            var permissions = EditorPermissions
                .Select((permission, index) => new Permission
                {
                    Id = index + 1,
                    Name = permission.Name,
                    Scope = permission.Scope,
                    Module = permission.Name == DisabledModulePermission ? disabledModule : null
                })
                .ToList();

            dbContext.Modules.Add(disabledModule);
            dbContext.Permissions.AddRange(permissions);
            dbContext.Roles.Add(new ApplicationRole
            {
                Id = EditedRoleId,
                Name = "Edited",
                OrganizationId = EditorOrganization.OrganizationId,
                Permissions = permissions.Where(permission => rolePermissions.Contains(permission.Name)).ToList()
            });
            await dbContext.SaveChangesAsync(false);
            dbContext.ChangeTracker.Clear();

            var uow = new UnitOfWork2(dbContext);
            var permissionService = new PermissionService(uow, Substitute.For<ICustomCache<string, IEnumerable<string>>>());

            return (new RoleService(uow, permissionService), dbContext);
        }

        private static async Task<List<int>> GetRolePermissionIdsAsync(ShroomsDbContext dbContext)
        {
            return await dbContext.Permissions
                .Where(permission => permission.Roles.Any(role => role.Id == EditedRoleId))
                .Select(permission => permission.Id)
                .ToListAsync();
        }

        private static async Task<List<string>> GetPermissionNamesAsync(ShroomsDbContext dbContext, IEnumerable<int> permissionIds)
        {
            var ids = permissionIds.ToList();

            return await dbContext.Permissions
                .Where(permission => ids.Contains(permission.Id))
                .Select(permission => permission.Name)
                .ToListAsync();
        }

        private void MockRoles()
        {
            var user1 = new ApplicationUser { Id = "userId1", FirstName = "first1", LastName = "last1" };
            var user2 = new ApplicationUser { Id = "userId2", FirstName = "first2", LastName = "last2" };
            var user3 = new ApplicationUser { Id = "userId3", FirstName = "first3", LastName = "last3" };

            var users = new List<ApplicationUser> { user1, user2, user3 }.AsQueryable();

            var userRoles = new List<IdentityUserRole<string>>
            {
                new() { UserId = "userId1", RoleId = "roleId1" },
                new() { UserId = "userId2", RoleId = "roleId1" },
                new() { UserId = "userId3", RoleId = "roleId2" }
            }.AsQueryable();

            var roles = new List<ApplicationRole>
            {
                new()
                {
                    Id = "roleId1",
                    Name = "Test1"
                },
                new()
                {
                    Id = "roleId2",
                    Name = "Test2"
                }
            }.AsQueryable();

            _permissionService.GetGroupNamesAsync(1).Returns(
                new List<PermissionGroupDto>
                {
                    new()
                    {
                        Name = "permission1"
                    },
                    new()
                    {
                        Name = "permission2"
                    },
                    new()
                    {
                        Name = "permission3"
                    }
                });

            _permissionService.GetRolePermissionsAsync("roleId1", 1).Returns(
                new List<PermissionDto>
                {
                    new()
                    {
                        Name = "PERMISSION1_BASIC",
                        Scope = PermissionScopes.Basic
                    },
                    new()
                    {
                        Name = "PERMISSION2_ADMIN",
                        Scope = PermissionScopes.Administration
                    }
                });

            _roleDbSet.SetDbSetDataForAsync(roles);
            _usersDbSet.SetDbSetDataForAsync(users);
            _userRolesDbSet.SetDbSetDataForAsync(userRoles);
        }

        private void MockRolesForAutocomplete()
        {
            var roles = new List<ApplicationRole>
            {
                new()
                {
                    Id = "roleId1",
                    Name = "Test1",
                    OrganizationId = 2
                },
                new()
                {
                    Id = "roleId2",
                    Name = "Test2",
                    OrganizationId = 2
                },
                new()
                {
                    Id = "roleId3",
                    Name = "Test12",
                    OrganizationId = 2
                }
            }.AsQueryable();

            _roleDbSet.SetDbSetDataForAsync(roles);
        }
    }
}
