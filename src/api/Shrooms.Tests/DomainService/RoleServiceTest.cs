using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;
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
