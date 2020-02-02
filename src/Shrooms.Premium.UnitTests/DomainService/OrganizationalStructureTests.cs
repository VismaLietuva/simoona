using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Domain.Services.Roles;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.OrganizationalStructure;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.Premium.UnitTests.DomainService
{
    [TestFixture]
    public class OrganizationalStructureTests
    {
        private IDbSet<ApplicationUser> _usersDbSet;
        private IOrganizationalStructureService _organizationalStructureService;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = Substitute.For<IDbSet<ApplicationUser>>();
            _usersDbSet.SetDbSetData(MockUsers());
            uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            var roleService = Substitute.For<IRoleService>();
            MockRoleService(roleService);
            _organizationalStructureService = new OrganizationalStructureService(uow, roleService);
        }

        [Test]
        public void Should_Return_If_Result_Has_Incorrect_Info()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 1,
                UserId = "0"
            };

            var result = _organizationalStructureService.GetOrganizationalStructure(userAndOrg);
            Assert.AreEqual("Name1 Surname1", result.FullName);
            Assert.AreEqual("Name2 Surname2", result.Children.First().FullName);
            Assert.AreEqual("Name3 Surname3", result.Children.ToArray()[1].FullName);
            Assert.AreEqual("Name4 Surname4", result.Children.First().Children.First().FullName);
            Assert.AreEqual("Name6 Surname6", result.Children.ToArray()[1].Children.First().FullName);
        }

        private static void MockRoleService(IRoleService roleService)
        {
            roleService.ExcludeUsersWithRole(Roles.NewUser).Returns(x => true);
        }

        private IQueryable<ApplicationUser> MockUsers()
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
