using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;
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
        private ICustomCache<string, IEnumerable<string>> _permissionCache;
        private IDbSet<ApplicationUser> _usersDbSet;
        private IDbSet<Permission> _permissionsDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = Substitute.For<IDbSet<ApplicationUser>>();
            _permissionsDbSet = Substitute.For<IDbSet<Permission>>();

            uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);
            uow.GetDbSet<Permission>().Returns(_permissionsDbSet);

            _permissionCache = Substitute.For<ICustomCache<string, IEnumerable<string>>>();

            _permissionService = new PermissionService(uow, _permissionCache);
        }

        [Test]
        public void Should_Get_Permissions_Separated_To_Groups()
        {
            MockPermissions();
            var permissionGroups = _permissionService.GetGroupNames(1).ToList();

            Assert.AreEqual(2, permissionGroups.Count());
            Assert.AreEqual("test1", permissionGroups.ToArray()[0].Name);
            Assert.AreEqual("test2", permissionGroups.ToArray()[1].Name);
        }

        [Test]
        public void Should_Get_Permissions_Separated_To_Groups_Filtered_By_Organization()
        {
            MockPermissions();
            var permissionGroups = _permissionService.GetGroupNames(2).ToList();

            Assert.AreEqual(2, permissionGroups.Count());
            Assert.AreEqual("test2", permissionGroups.ToArray()[0].Name);
            Assert.AreEqual("test3", permissionGroups.ToArray()[1].Name);
        }

        [Test]
        public void Should_Get_Admin_User_Permissions()
        {
            MockPermissions();
            var permissionGroups = _permissionService.GetUserPermissions("UserId1", 1).ToList();

            Assert.AreEqual(4, permissionGroups.Count());
            Assert.AreEqual("TEST1_BASIC", permissionGroups.ToArray()[0]);
            Assert.AreEqual("TEST1_ADMIN", permissionGroups.ToArray()[1]);
        }

        [Test]
        public void Should_Get_Admin_User_Permissions_From_Cache()
        {
            _permissionCache.TryGetValue("UserId1", out _).Returns(x =>
            {
                x[1] = new List<string>()
                {
                    "TEST1_BASIC",
                    "TEST1_ADMIN"
                };
                return true;
            });

            var permissionGroups = _permissionService.GetUserPermissions("UserId1", 1).ToList();

            Assert.AreEqual(2, permissionGroups.Count());
            Assert.AreEqual("TEST1_BASIC", permissionGroups.ToArray()[0]);
            Assert.AreEqual("TEST1_ADMIN", permissionGroups.ToArray()[1]);
        }

        [Test]
        public void Should_Try_Remove_User_Permissions_From_Cache()
        {
            _permissionService.RemoveCache("UserId1");

            _permissionCache.Received().TryRemoveEntry("UserId1");
        }

        [Test]
        public void Should_Get_Simple_User_Permissions()
        {
            MockPermissions();
            var permissionGroups = _permissionService.GetUserPermissions("UserId2", 1).ToList();

            Assert.AreEqual(2, permissionGroups.Count());
            Assert.AreEqual("TEST1_BASIC", permissionGroups.ToArray()[0]);
            Assert.AreEqual("TEST2_BASIC", permissionGroups.ToArray()[1]);
        }

        [Test]
        public void Should_Get_Admin_Role_Permissions()
        {
            MockPermissions();
            var permissionGroups = _permissionService.GetRolePermissions("AdminId", 1).ToList();

            Assert.AreEqual(4, permissionGroups.Count());
            Assert.AreEqual("TEST1_BASIC", permissionGroups.ToArray()[0].Name);
            Assert.AreEqual("TEST1_ADMIN", permissionGroups.ToArray()[1].Name);
        }

        [Test]
        public void Should_Get_User_Role_Permissions()
        {
            MockPermissions();
            var permissionGroups = _permissionService.GetRolePermissions("UserId", 1).ToList();

            Assert.AreEqual(2, permissionGroups.Count());
            Assert.AreEqual("TEST1_BASIC", permissionGroups.ToArray()[0].Name);
            Assert.AreEqual("TEST2_BASIC", permissionGroups.ToArray()[1].Name);
        }

        [Test]
        public void Should_Return_That_User_Is_Permitted()
        {
            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 1,
                UserId = "userId"
            };

            MockUserPermission();

            var hasPermission = _permissionService.UserHasPermission(userAndOrg, "TEST1_BASIC");

            Assert.AreEqual(true, hasPermission);
        }

        [Test]
        public void Should_Return_That_User_Is_Not_Permitted()
        {
            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 2,
                UserId = "userId"
            };

            MockUserPermission();

            var hasPermission = _permissionService.UserHasPermission(userAndOrg, "TEST1_BASIC");

            Assert.AreEqual(false, hasPermission);
        }

        private void MockUserPermission()
        {
            var adminRole = Substitute.For<ApplicationRole>();
            adminRole.Users.Returns(new List<IdentityUserRole>()
            {
                new IdentityUserRole
                {
                    UserId = "userId"
                },
            });

            var organizationId1 = new List<Organization>
            {
                new Organization
                {
                    Id = 1
                }
            };

            var permissions = new List<Permission>
            {
                new Permission
                {
                    Id = 1,
                    Name = "TEST1_BASIC",
                    Scope = PermissionScopes.Basic,
                    ModuleId = 1,
                    Module = new Module()
                    {
                        Organizations = organizationId1
                    },
                    Roles = new List<ApplicationRole>
                    {
                        adminRole
                    }
                },
            }.AsQueryable();

            _permissionsDbSet.SetDbSetData(permissions);
        }

        private void MockPermissions()
        {
            var adminRole = Substitute.For<ApplicationRole>();
            adminRole.Id = "AdminId";
            adminRole.Users.Returns(new List<IdentityUserRole>()
            {
                new IdentityUserRole
                {
                    RoleId = "AdminId",
                    UserId = "UserId1"
                },
            });

            var userRole = Substitute.For<ApplicationRole>();
            userRole.Id = "UserId";
            userRole.Users.Returns(new List<IdentityUserRole>()
            {
                new IdentityUserRole
                {
                    RoleId = "UserId",
                    UserId = "UserId2"
                }
            });

            var organizationId1 = new List<Organization>
            {
                new Organization
                {
                    Id = 1
                }
            };

            var organizationId2 = new List<Organization>
            {
                new Organization
                {
                    Id = 2
                }
            };

            var permissions = new List<Permission>
            {
                new Permission
                {
                    Id = 1,
                    Name = "TEST1_BASIC",
                    Scope = PermissionScopes.Basic,
                    ModuleId = 1,
                    Module = new Module()
                    {
                        Organizations = organizationId1
                    },
                    Roles = new List<ApplicationRole>
                    {
                        adminRole,
                        userRole
                    }
                },
                new Permission
                {
                    Id = 2,
                    Name = "TEST1_ADMIN",
                    Scope = PermissionScopes.Administration,
                    ModuleId = 1,
                    Module = new Module()
                    {
                        Organizations = organizationId1
                    },
                    Roles = new List<ApplicationRole>
                    {
                        adminRole
                    }
                },
                new Permission
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
                new Permission
                {
                    Id = 4,
                    Name = "TEST2_ADMIN",
                    Scope = PermissionScopes.Administration,
                    Roles = new List<ApplicationRole>
                    {
                        adminRole
                    }
                },
                new Permission
                {
                    Id = 5,
                    Name = "TEST3_BASIC",
                    Scope = PermissionScopes.Basic,
                    ModuleId = 1,
                    Module = new Module()
                    {
                        Organizations = organizationId2
                    },
                    Roles = new List<ApplicationRole>
                    {
                        adminRole,
                        userRole
                    }
                },
                new Permission
                {
                    Id = 6,
                    Name = "TEST3_ADMIN",
                    Scope = PermissionScopes.Administration,
                    ModuleId = 1,
                    Module = new Module()
                    {
                        Organizations = organizationId2
                    },
                    Roles = new List<ApplicationRole>
                    {
                        adminRole
                    }
                },
            }.AsQueryable();

            _permissionsDbSet.SetDbSetData(permissions);
        }
    }
}
