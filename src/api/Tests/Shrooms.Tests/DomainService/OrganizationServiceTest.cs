using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Constants.Authorization;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.Roles;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.DomainExceptions.Exceptions.Organization;
using Shrooms.EntityModels.Models;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.UnitTests.DomainService
{
    [TestFixture]
    public class OrganizationServiceTest
    {
        private IOrganizationService _organizationService;
        private IRoleService _roleService;
        private IDbSet<Organization> _organizationsDbSet;
        private IDbSet<ApplicationUser> _usersDbSet;

        [SetUp]
        public void Init()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            _roleService = Substitute.For<IRoleService>();

            _organizationsDbSet = uow.MockDbSet<Organization>();
            _usersDbSet = uow.MockDbSet<ApplicationUser>();

            _organizationService = new OrganizationService(uow, _roleService);
        }

        [Test]
        public void Should_Get_Organization_By_Id()
        {
            _organizationsDbSet.Find(1).Returns(new Organization { Id = 1, ShortName = "Organization1" });

            var response = _organizationService.GetOrganizationById(1);

            Assert.AreEqual(response.Id, 1);
            Assert.AreEqual(response.ShortName, "Organization1");
        }

        [Test]
        public void Should_Get_Organization_By_Name()
        {
            var organizations = new List<Organization>
            {
                new Organization { Id = 1, ShortName = "Organization1" },
                new Organization { Id = 2, ShortName = "Organization2" }
            };
            _organizationsDbSet.SetDbSetData(organizations.AsQueryable());

            var response = _organizationService.GetOrganizationByName("Organization2");

            Assert.AreEqual(response.Id, 2);
            Assert.AreEqual(response.ShortName, "Organization2");
        }

        [Test]
        public void Should_Get_Organization_Host_Name_By_Organization_Name()
        {
            var organizations = new List<Organization>
            {
                new Organization { Id = 1, ShortName = "Organization1", HostName = "Host1" },
                new Organization { Id = 2, ShortName = "Organization2", HostName = "Host2" }
            };
            _organizationsDbSet.SetDbSetData(organizations);

            var response = _organizationService.GetOrganizationHostName("Organization2");

            Assert.AreEqual(response, "Host2");
        }

        [Test]
        public void Should_Return_Organization_Email_Domain_Restriction()
        {
            var organizations = new List<Organization>
            {
                new Organization { Id = 1, ShortName = "Organization1", HostName = "Host1" },
                new Organization { Id = 2, ShortName = "Organization2", HostName = "Host2", HasRestrictedAccess = true }
            };
            _organizationsDbSet.SetDbSetData(organizations);

            var response = _organizationService.GetOrganizationHostName("Organization2");

            Assert.AreEqual(response, "Host2");
        }

        [Test]
        public void Should_Throw_If_No_Organization_When_Checking_Email_Domain_Restriction()
        {
            var organizations = new List<Organization>
            {
                new Organization { Id = 1, ShortName = "Organization1", HostName = "Host1" }
            };

            _organizationsDbSet.SetDbSetData(organizations);

            Assert.Throws<InvalidOrganizationException>(() => _organizationService.HasOrganizationEmailDomainRestriction("Organization2"));
        }

        [Test]
        public void Should_Return_User_Organization()
        {
            var organization = new Organization { Id = 1, ShortName = "Organization1", HostName = "Host1" };
            var user = new ApplicationUser { Id = "user1", Organization = organization };

            _usersDbSet.Find("user1").Returns(user);

            var response = _organizationService.GetUserOrganization(new ApplicationUser { Id = "user1" });

            Assert.AreEqual(response.ShortName, "Organization1");
        }

        [Test]
        public void Should_Return_Invalid_Organization_Host()
        {
            var organizations = new List<Organization>
            {
                new Organization { Id = 1, ShortName = "Organization1", HostName = "host1.com", HasRestrictedAccess = true },
                new Organization { Id = 2, ShortName = "Organization2", HostName = "host2.com" }
            };
            _organizationsDbSet.SetDbSetData(organizations);

            var response = _organizationService.IsOrganizationHostValid("organization1@host1c.om", "Organization1");

            Assert.IsFalse(response);
        }

        [Test]
        public void Should_Return_Valid_Organization_Host()
        {
            var organizations = new List<Organization>
            {
                new Organization { Id = 1, ShortName = "Organization1", HostName = "host1.com", HasRestrictedAccess = true },
                new Organization { Id = 2, ShortName = "Organization2", HostName = "host2.com" }
            };
            _organizationsDbSet.SetDbSetData(organizations);

            var response = _organizationService.IsOrganizationHostValid("organization1@host1.com", "Organization1");

            Assert.IsTrue(response);
        }

        [Test]
        public void Should_Return_Requires_User_Confirmation()
        {
            var organizations = new List<Organization>
            {
                new Organization { Id = 1, ShortName = "Organization1", RequiresUserConfirmation = true },
                new Organization { Id = 2, ShortName = "Organization2" }
            };
            _organizationsDbSet.SetDbSetData(organizations);

            var response = _organizationService.RequiresUserConfirmation(1);

            Assert.IsTrue(response);
        }

        [Test]
        public void Should_Get_Managing_Director()
        {
            var user = new ApplicationUser { Id = "user1", IsManagingDirector = true, FirstName = "John", LastName = "Doe", OrganizationId = 1 };

            _usersDbSet.SetDbSetData(new List<ApplicationUser> { user });

            var response = _organizationService.GetManagingDirector(1);

            Assert.AreEqual(response.UserId, "user1");
            Assert.AreEqual(response.FullName, "John Doe");
        }

        [Test]
        public void Should_Throw_Exception_If_User_Has_No_Permission_To_Set_Managing_Director()
        {
            var user = new ApplicationUser { Id = "user1", IsManagingDirector = true, FirstName = "John", LastName = "Doe", OrganizationId = 1 };
            var userAndOrg = new UserAndOrganizationDTO { OrganizationId = 1, UserId = "user1" };
            _roleService.HasRole("user1", Roles.Manager).Returns(false);

            Assert.Throws<ValidationException>(() => _organizationService.SetManagingDirector("user1", userAndOrg));
        }

        [Test]
        public void Should_To_Set_Managing_Director()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", IsManagingDirector = false, FirstName = "John", LastName = "Doe", OrganizationId = 1 },
                new ApplicationUser { Id = "user2", IsManagingDirector = true, FirstName = "John", LastName = "Doe", OrganizationId = 1 },
                new ApplicationUser { Id = "user3", IsManagingDirector = true, FirstName = "John", LastName = "Doe", OrganizationId = 1 }
            };

            _usersDbSet.SetDbSetData(users);

            var userAndOrg = new UserAndOrganizationDTO { OrganizationId = 1, UserId = "user1" };
            _roleService.HasRole("user1", Roles.Manager).Returns(true);

            _organizationService.SetManagingDirector("user1", userAndOrg);

            Assert.IsTrue(users[0].IsManagingDirector);
            Assert.IsFalse(users[1].IsManagingDirector);
            Assert.IsFalse(users[2].IsManagingDirector);
        }
    }
}
