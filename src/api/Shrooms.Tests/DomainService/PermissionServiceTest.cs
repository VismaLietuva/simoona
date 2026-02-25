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
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class PermissionServiceTest
    {
        private IPermissionService _permissionService;
        private ICustomCache<string, IList<string>> _permissionCache;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<Permission> _permissionsDbSet;
        private DbSet<IdentityUserRole<string>> _userRolesDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IAsyncEnumerable<ApplicationUser>>();
            _permissionsDbSet = Substitute.For<DbSet<Permission>, IQueryable<Permission>, IAsyncEnumerable<Permission>>();
            _userRolesDbSet = Substitute.For<DbSet<IdentityUserRole<string>>, IQueryable<IdentityUserRole<string>>, IAsyncEnumerable<IdentityUserRole<string>>>();

            uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);
            uow.GetDbSet<Permission>().Returns(_permissionsDbSet);
            uow.GetDbSet<IdentityUserRole<string>>().Returns(_userRolesDbSet);

            _permissionCache = Substitute.For<ICustomCache<string, IList<string>>>();

            _permissionService = new PermissionService(uow, _permissionCache);
        }

        [Test]
        public async Task Should_Get_Permissions_Separated_To_Groups()
        {
            MockPermissions();
            var permissionGroups = (await _permissionService.GetGroupNamesAsync(1)).ToList();

            Assert.That(permissionGroups.Count, Is.EqualTo(2));
            Assert.That(permissionGroups.ToArray()[0].Name, Is.EqualTo("test1"));
            Assert.That(permissionGroups.ToArray()[1].Name, Is.EqualTo("test2"));
        }

        [Test]
        public async Task Should_Get_Permissions_Separated_To_Groups_Filtered_By_Organization()
        {
            MockPermissions();
            var permissionGroups = (await _permissionService.GetGroupNamesAsync(2)).ToList();

            Assert.That(permissionGroups.Count, Is.EqualTo(2));
            Assert.That(permissionGroups.ToArray()[0].Name, Is.EqualTo("test2"));
            Assert.That(permissionGroups.ToArray()[1].Name, Is.EqualTo("test3"));
        }

        [Test]
        public async Task Should_Get_Admin_User_Permissions()
        {
            MockPermissions();
            var permissionGroups = (await _permissionService.GetUserPermissionsAsync("UserId1", 1)).ToList();

            Assert.That(permissionGroups.Count, Is.EqualTo(4));
            Assert.That(permissionGroups.ToArray()[0], Is.EqualTo("TEST1_BASIC"));
            Assert.That(permissionGroups.ToArray()[1], Is.EqualTo("TEST1_ADMIN"));
        }

        [Test]
        public async Task Should_Get_Admin_User_Permissions_From_Cache()
        {
            _permissionCache.TryGetValue("UserId1", out _).Returns(x =>
            {
                x[1] = new List<string>
                {
                    "TEST1_BASIC",
                    "TEST1_ADMIN"
                };
                return true;
            });

            var permissionGroups = (await _permissionService.GetUserPermissionsAsync("UserId1", 1)).ToList();

            Assert.That(permissionGroups.Count, Is.EqualTo(2));
            Assert.That(permissionGroups.ToArray()[0], Is.EqualTo("TEST1_BASIC"));
            Assert.That(permissionGroups.ToArray()[1], Is.EqualTo("TEST1_ADMIN"));
        }

        [Test]
        public void Should_Try_Remove_User_Permissions_From_Cache()
        {
            _permissionService.RemoveCache("UserId1");

            _permissionCache.Received().TryRemoveEntry("UserId1");
        }

        [Test]
        public async Task Should_Get_Simple_User_Permissions()
        {
            MockPermissions();
            var permissionGroups = (await _permissionService.GetUserPermissionsAsync("UserId2", 1)).ToList();

            Assert.That(permissionGroups.Count, Is.EqualTo(2));
            Assert.That(permissionGroups.ToArray()[0], Is.EqualTo("TEST1_BASIC"));
            Assert.That(permissionGroups.ToArray()[1], Is.EqualTo("TEST2_BASIC"));
        }

        [Test]
        public async Task Should_Get_Admin_Role_Permissions()
        {
            MockPermissions();
            var permissionGroups = (await _permissionService.GetRolePermissionsAsync("AdminId", 1)).ToList();

            Assert.That(permissionGroups.Count, Is.EqualTo(4));
            Assert.That(permissionGroups.ToArray()[0].Name, Is.EqualTo("TEST1_BASIC"));
            Assert.That(permissionGroups.ToArray()[1].Name, Is.EqualTo("TEST1_ADMIN"));
        }

        [Test]
        public async Task Should_Get_User_Role_Permissions()
        {
            MockPermissions();
            var permissionGroups = (await _permissionService.GetRolePermissionsAsync("UserId", 1)).ToList();

            Assert.That(permissionGroups.Count, Is.EqualTo(2));
            Assert.That(permissionGroups.ToArray()[0].Name, Is.EqualTo("TEST1_BASIC"));
            Assert.That(permissionGroups.ToArray()[1].Name, Is.EqualTo("TEST2_BASIC"));
        }

        [Test]
        public async Task Should_Return_That_User_Is_Permitted()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1,
                UserId = "userId"
            };

            MockUserPermission();

            var hasPermission = await _permissionService.UserHasPermissionAsync(userAndOrg, "TEST1_BASIC");

            Assert.That(hasPermission, Is.EqualTo(true));
        }

        [Test]
        public async Task Should_Return_That_User_Is_Not_Permitted()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "userId"
            };

            MockUserPermission();

            var hasPermission = await _permissionService.UserHasPermissionAsync(userAndOrg, "TEST1_BASIC");

            Assert.That(hasPermission, Is.EqualTo(false));
        }

        private void MockUserPermission()
        {
            var adminRole = new ApplicationRole { Id = "adminRoleId" };

            var organizationId1 = new List<Organization>
            {
                new()
                {
                    Id = 1
                }
            };

            var permissions = new List<Permission>
            {
                new()
                {
                    Id = 1,
                    Name = "TEST1_BASIC",
                    Scope = PermissionScopes.Basic,
                    ModuleId = 1,
                    Module = new Module
                    {
                        Organizations = organizationId1
                    },
                    Roles = new List<ApplicationRole>
                    {
                        adminRole
                    }
                }
            }.AsQueryable();

            var userRoles = new List<IdentityUserRole<string>>
            {
                new() { UserId = "userId", RoleId = "adminRoleId" }
            }.AsQueryable();

            _permissionsDbSet.SetDbSetDataForAsync(permissions);
            _userRolesDbSet.SetDbSetDataForAsync(userRoles);
        }

        private void MockPermissions()
        {
            var adminRole = new ApplicationRole { Id = "AdminId" };
            var userRole = new ApplicationRole { Id = "UserId" };

            var userRoles = new List<IdentityUserRole<string>>
            {
                new() { RoleId = "AdminId", UserId = "UserId1" },
                new() { RoleId = "UserId", UserId = "UserId2" }
            }.AsQueryable();

            var organizationId1 = new List<Organization>
            {
                new()
                {
                    Id = 1
                }
            };

            var organizationId2 = new List<Organization>
            {
                new()
                {
                    Id = 2
                }
            };

            var permissions = new List<Permission>
            {
                new()
                {
                    Id = 1,
                    Name = "TEST1_BASIC",
                    Scope = PermissionScopes.Basic,
                    ModuleId = 1,
                    Module = new Module
                    {
                        Organizations = organizationId1
                    },
                    Roles = new List<ApplicationRole>
                    {
                        adminRole,
                        userRole
                    }
                },
                new()
                {
                    Id = 2,
                    Name = "TEST1_ADMIN",
                    Scope = PermissionScopes.Administration,
                    ModuleId = 1,
                    Module = new Module
                    {
                        Organizations = organizationId1
                    },
                    Roles = new List<ApplicationRole>
                    {
                        adminRole
                    }
                },
                new()
                {
                    Id = 3,
                    Name = "TEST2_BASIC",
                    Scope = PermissionScopes.Basic,
                    Roles = new List<ApplicationRole>
                    {
                        adminRole,
                        userRole
                    }
                },
                new()
                {
                    Id = 4,
                    Name = "TEST2_ADMIN",
                    Scope = PermissionScopes.Administration,
                    Roles = new List<ApplicationRole>
                    {
                        adminRole
                    }
                },
                new()
                {
                    Id = 5,
                    Name = "TEST3_BASIC",
                    Scope = PermissionScopes.Basic,
                    ModuleId = 1,
                    Module = new Module
                    {
                        Organizations = organizationId2
                    },
                    Roles = new List<ApplicationRole>
                    {
                        adminRole,
                        userRole
                    }
                },
                new()
                {
                    Id = 6,
                    Name = "TEST3_ADMIN",
                    Scope = PermissionScopes.Administration,
                    ModuleId = 1,
                    Module = new Module
                    {
                        Organizations = organizationId2
                    },
                    Roles = new List<ApplicationRole>
                    {
                        adminRole
                    }
                }
            }.AsQueryable();

            _permissionsDbSet.SetDbSetDataForAsync(permissions);
            _userRolesDbSet.SetDbSetDataForAsync(userRoles);
        }
    }
}
