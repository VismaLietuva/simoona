using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.Roles;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.Mocks;
using Shrooms.Tests.ModelMappings;

namespace Shrooms.Tests.Controllers.WebApi
{
    /// <summary>
    /// Role/Get, Role/Put and Role/Post against an in-memory database, the way the role editor calls them.
    /// </summary>
    [TestFixture]
    public class RolesControllerTests
    {
        private const int OrganizationId = 1;
        private const string EditedRoleId = "edited-role";
        private const string DisabledModulePermission = "LOTTERY_BASIC";
        private const string MemberId = "member";

        private static readonly string[] EditedRolePermissions =
        {
            // Admin without basic: re-granting kudos from its scope would add KUDOS_BASIC.
            AdministrationPermissions.Kudos,
            AdministrationPermissions.KudosShop,
            AdministrationPermissions.Role,
            // A module the organization does not use: Get does not show it.
            DisabledModulePermission
        };

        private ShroomsDbContext _dbContext;
        private ShroomsUserManager _userManager;
        private RolesController _rolesController;

        [SetUp]
        public async Task TestInitializer()
        {
            var options = new DbContextOptionsBuilder<ShroomsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ShroomsDbContext(options);

            var disabledModule = new Module { Id = 1, Name = "Lottery", Organizations = new List<Organization>() };
            var permissions = new[]
                {
                    (BasicPermissions.Kudos, PermissionScopes.Basic),
                    (AdministrationPermissions.Kudos, PermissionScopes.Administration),
                    (AdministrationPermissions.KudosShop, PermissionScopes.Administration),
                    (AdministrationPermissions.Role, PermissionScopes.Administration),
                    (DisabledModulePermission, PermissionScopes.Basic)
                }
                .Select((permission, index) => new Permission
                {
                    Id = index + 1,
                    Name = permission.Item1,
                    Scope = permission.Item2,
                    Module = permission.Item1 == DisabledModulePermission ? disabledModule : null
                })
                .ToList();

            _dbContext.Modules.Add(disabledModule);
            _dbContext.Permissions.AddRange(permissions);
            _dbContext.Roles.Add(new ApplicationRole
            {
                Id = EditedRoleId,
                Name = "Edited",
                OrganizationId = OrganizationId,
                Permissions = permissions.Where(permission => EditedRolePermissions.Contains(permission.Name)).ToList()
            });
            var member = new ApplicationUser { Id = MemberId, UserName = MemberId, FirstName = "Role", LastName = "Member", OrganizationId = OrganizationId };
            _dbContext.Users.Add(member);
            _dbContext.UserRoles.Add(new IdentityUserRole<string> { UserId = MemberId, RoleId = EditedRoleId });
            await _dbContext.SaveChangesAsync(false);
            _dbContext.ChangeTracker.Clear();

            var appSettings = Substitute.For<IApplicationSettings>();
            appSettings.DefaultOrganizationId.Returns(OrganizationId);
            var permissionsCache = Substitute.For<ICustomCache<string, IEnumerable<string>>>();
            var unitOfWork2 = new UnitOfWork2(_dbContext);
            var permissionService = new PermissionService(unitOfWork2, permissionsCache);

            _userManager = MockIdentity.MockUserManager(MockIdentity.MockShroomsUserStore(_dbContext), _dbContext);
            _userManager.GetUsersInRoleAsync("Edited").Returns(new List<ApplicationUser> { member });
            _userManager.GetUsersInRoleAsync(Arg.Is<string>(name => name != "Edited")).Returns(new List<ApplicationUser>());
            _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
            _userManager.RemoveFromRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
            var roleManager = new ShroomsRoleManager(
                new RoleStore<ApplicationRole>(_dbContext),
                Array.Empty<IRoleValidator<ApplicationRole>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                Substitute.For<ILogger<RoleManager<ApplicationRole>>>());

            _rolesController = new RolesController(
                ModelMapper.Create(),
                new EfUnitOfWork(_dbContext, appSettings),
                permissionService,
                permissionsCache,
                new RoleService(unitOfWork2, permissionService),
                _userManager,
                roleManager);
            _rolesController.SetUpControllerForTesting();
        }

        [TearDown]
        public void TestCleanup()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Put_WhatGetReturned_KeepsTheRolePermissions()
        {
            var role = (await _rolesController.Get(EditedRoleId)).GetContent<RoleDetailsViewModel>();

            await _rolesController.Put(ToRequest(role));

            Assert.That(await GetRolePermissionNamesAsync(EditedRoleId), Is.EquivalentTo(EditedRolePermissions));
            await _userManager.DidNotReceiveWithAnyArgs().AddToRoleAsync(default, default);
            await _userManager.DidNotReceiveWithAnyArgs().RemoveFromRoleAsync(default, default);
        }

        [Test]
        public async Task Put_GroupChangedToBasic_GrantsItsBasicPermissionsAndKeepsTheRest()
        {
            var role = (await _rolesController.Get(EditedRoleId)).GetContent<RoleDetailsViewModel>();
            var request = ToRequest(role);
            request.Permissions = request.Permissions
                .Select(group => group.Name == "kudos" ? new PermissionGroupViewModel { Name = group.Name, ActiveScope = PermissionScopes.Basic } : group)
                .ToList();

            await _rolesController.Put(request);

            Assert.That(
                await GetRolePermissionNamesAsync(EditedRoleId),
                Is.EquivalentTo(new[] { BasicPermissions.Kudos, AdministrationPermissions.KudosShop, AdministrationPermissions.Role, DisabledModulePermission }));
        }

        [Test]
        public async Task Put_WithoutPermissions_SavesTheNameAndKeepsThePermissions()
        {
            await _rolesController.Put(new RoleMiniViewModel { Id = EditedRoleId, Name = "Renamed", Users = new List<ApplicationUserViewModel>() });

            var role = await _dbContext.Roles.AsNoTracking().SingleAsync(r => r.Id == EditedRoleId);
            Assert.That(role.Name, Is.EqualTo("Renamed"));
            Assert.That(await GetRolePermissionNamesAsync(EditedRoleId), Is.EquivalentTo(EditedRolePermissions));
        }

        [Test]
        public async Task Put_WithOnlyIdAndName_KeepsThePermissionsAndTheMembers()
        {
            await _rolesController.Put(new RoleMiniViewModel { Id = EditedRoleId, Name = "Edited" });

            Assert.That(await GetRolePermissionNamesAsync(EditedRoleId), Is.EquivalentTo(EditedRolePermissions));
            await _userManager.DidNotReceiveWithAnyArgs().RemoveFromRoleAsync(default, default);
        }

        [Test]
        public async Task Put_WithEmptyUsers_RemovesTheMembers()
        {
            await _rolesController.Put(new RoleMiniViewModel { Id = EditedRoleId, Name = "Edited", Users = new List<ApplicationUserViewModel>() });

            await _userManager.Received(1).RemoveFromRoleAsync(Arg.Is<ApplicationUser>(user => user.Id == MemberId), "Edited");
        }

        [Test]
        public async Task Post_GroupSetToAdmin_CreatesTheRoleWithTheWholeGroupOnly()
        {
            var result = await _rolesController.Post(new RoleMiniViewModel
            {
                Name = "New",
                Permissions = new[]
                {
                    new PermissionGroupViewModel { Name = "kudos", ActiveScope = PermissionScopes.Administration },
                    new PermissionGroupViewModel { Name = "kudosshop", ActiveScope = string.Empty },
                    new PermissionGroupViewModel { Name = "roles", ActiveScope = string.Empty }
                },
                Users = new List<ApplicationUserViewModel>()
            });

            Assert.That(
                await GetRolePermissionNamesAsync(result.GetContent<string>()),
                Is.EquivalentTo(new[] { BasicPermissions.Kudos, AdministrationPermissions.Kudos }));
        }

        private static RoleMiniViewModel ToRequest(RoleDetailsViewModel role)
        {
            // The role editor sends back what Get returned, every group and member included.
            return new RoleMiniViewModel
            {
                Id = role.Id,
                Name = role.Name,
                Permissions = role.Permissions.Select(group => new PermissionGroupViewModel { Name = group.Name, ActiveScope = group.ActiveScope }).ToList(),
                Users = role.Users.Select(user => new ApplicationUserViewModel { Id = user.Id }).ToList()
            };
        }

        private async Task<List<string>> GetRolePermissionNamesAsync(string roleId)
        {
            return await _dbContext.Permissions
                .AsNoTracking()
                .Where(permission => permission.Roles.Any(role => role.Id == roleId))
                .Select(permission => permission.Name)
                .ToListAsync();
        }
    }
}
