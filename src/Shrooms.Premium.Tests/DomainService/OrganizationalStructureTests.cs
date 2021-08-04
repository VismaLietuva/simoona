using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Roles;
using Shrooms.Premium.Domain.Services.OrganizationalStructure;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService
{
    [TestFixture]
    public class OrganizationalStructureTests
    {
        private DbSet<ApplicationUser> _usersDbSet;
        private IOrganizationalStructureService _organizationalStructureService;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IDbAsyncEnumerable<ApplicationUser>>();
            _usersDbSet.SetDbSetDataForAsync(MockUsers());
            uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            var roleService = Substitute.For<IRoleService>();
            MockRoleService(roleService);
            _organizationalStructureService = new OrganizationalStructureService(uow, roleService);
        }

        [Test]
        public async Task Should_Return_If_Result_Has_Incorrect_Info()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 1,
                UserId = "0"
            };

            var result = await _organizationalStructureService.GetOrganizationalStructureAsync(userAndOrg);
            Assert.AreEqual("Name1 Surname1", result.FullName);
            Assert.AreEqual("Name2 Surname2", result.Children.First().FullName);
            Assert.AreEqual("Name3 Surname3", result.Children.ToArray()[1].FullName);
            Assert.AreEqual("Name4 Surname4", result.Children.First().Children.First().FullName);
            Assert.AreEqual("Name6 Surname6", result.Children.ToArray()[1].Children.First().FullName);
        }

        private static void MockRoleService(IRoleService roleService)
        {
            var newRoleId = Guid.NewGuid().ToString();
            roleService.GetRoleIdByNameAsync(Roles.NewUser).Returns(newRoleId);
            roleService.ExcludeUsersWithRole(newRoleId).ReturnsForAnyArgs(x => true);
        }

        private static IQueryable<ApplicationUser> MockUsers()
        {
            return new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "1",
                    FirstName = "Name1",
                    LastName = "Surname1",
                    IsManagingDirector = true,
                    OrganizationId = 1
                },
                new ApplicationUser
                {
                    Id = "2",
                    FirstName = "Name2",
                    LastName = "Surname2",
                    IsManagingDirector = false,
                    ManagerId = "1",
                    OrganizationId = 1
                },
                new ApplicationUser
                {
                    Id = "3",
                    FirstName = "Name3",
                    LastName = "Surname3",
                    IsManagingDirector = false,
                    ManagerId = "1",
                    OrganizationId = 1
                },
                new ApplicationUser
                {
                    Id = "4",
                    FirstName = "Name4",
                    LastName = "Surname4",
                    IsManagingDirector = false,
                    ManagerId = "2",
                    OrganizationId = 1
                },
                new ApplicationUser
                {
                    Id = "5",
                    FirstName = "Name5",
                    LastName = "Surname5",
                    IsManagingDirector = false,
                    ManagerId = "4",
                    OrganizationId = 1
                },
                new ApplicationUser
                {
                    Id = "6",
                    FirstName = "Name6",
                    LastName = "Surname6",
                    IsManagingDirector = false,
                    ManagerId = "3",
                    OrganizationId = 1
                }
            }.AsQueryable();
        }
    }
}
