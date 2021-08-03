using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Exceptions.Exceptions.Organization;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.Roles;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class OrganizationServiceTest
    {
        private IOrganizationService _organizationService;
        private IRoleService _roleService;
        private DbSet<Organization> _organizationsDbSet;
        private DbSet<ApplicationUser> _usersDbSet;

        [SetUp]
        public void Init()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            _roleService = Substitute.For<IRoleService>();

            _organizationsDbSet = uow.MockDbSetForAsync<Organization>();
            _usersDbSet = uow.MockDbSetForAsync<ApplicationUser>();

            _organizationService = new OrganizationService(uow, _roleService);
        }

        [Test]
        public async Task Should_Get_Organization_By_Id()
        {
            _organizationsDbSet.FindAsync(1).Returns(new Organization { Id = 1, ShortName = "Organization1" });

            var response = await _organizationService.GetOrganizationByIdAsync(1);

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
            _organizationsDbSet.SetDbSetDataForAsync(organizations.AsQueryable());

            var response = _organizationService.GetOrganizationByName("Organization2");

            Assert.AreEqual(response.Id, 2);
            Assert.AreEqual(response.ShortName, "Organization2");
        }

        [Test]
        public async Task Should_Get_Organization_Host_Name_By_Organization_Name()
        {
            var organizations = new List<Organization>
            {
                new Organization { Id = 1, ShortName = "Organization1", HostName = "Host1" },
                new Organization { Id = 2, ShortName = "Organization2", HostName = "Host2" }
            };
            _organizationsDbSet.SetDbSetDataForAsync(organizations);

            var response = await _organizationService.GetOrganizationHostNameAsync("Organization2");

            Assert.AreEqual(response, "Host2");
        }

        [Test]
        public async Task Should_Return_Organization_Email_Domain_Restriction()
        {
            var organizations = new List<Organization>
            {
                new Organization { Id = 1, ShortName = "Organization1", HostName = "Host1" },
                new Organization { Id = 2, ShortName = "Organization2", HostName = "Host2", HasRestrictedAccess = true }
            };
            _organizationsDbSet.SetDbSetDataForAsync(organizations);

            var response = await _organizationService.GetOrganizationHostNameAsync("Organization2");

            Assert.AreEqual(response, "Host2");
        }

        [Test]
        public void Should_Throw_If_No_Organization_When_Checking_Email_Domain_Restriction()
        {
            var organizations = new List<Organization>
            {
                new Organization { Id = 1, ShortName = "Organization1", HostName = "Host1" }
            };

            _organizationsDbSet.SetDbSetDataForAsync(organizations);

            Assert.ThrowsAsync<InvalidOrganizationException>(async () => await _organizationService.HasOrganizationEmailDomainRestrictionAsync("Organization2"));
        }

        [Test]
        public async Task Should_Return_User_Organization()
        {
            var organization = new Organization { Id = 1, ShortName = "Organization1", HostName = "Host1" };
            var user = new ApplicationUser { Id = "user1", Organization = organization };

            _usersDbSet.FindAsync("user1").Returns(user);

            var response = await _organizationService.GetUserOrganizationAsync(new ApplicationUser { Id = "user1" });

            Assert.AreEqual(response.ShortName, "Organization1");
        }

        [Test]
        public async Task Should_Return_Invalid_Organization_Host()
        {
            var organizations = new List<Organization>
            {
                new Organization { Id = 1, ShortName = "Organization1", HostName = "host1.com", HasRestrictedAccess = true },
                new Organization { Id = 2, ShortName = "Organization2", HostName = "host2.com" }
            };

            _organizationsDbSet.SetDbSetDataForAsync(organizations);

            var response = await _organizationService.IsOrganizationHostValidAsync("organization1@host1c.om", "Organization1");

            Assert.IsFalse(response);
        }

        [Test]
        public async Task Should_Return_Valid_Organization_Host()
        {
            var organizations = new List<Organization>
            {
                new Organization { Id = 1, ShortName = "Organization1", HostName = "host1.com", HasRestrictedAccess = true },
                new Organization { Id = 2, ShortName = "Organization2", HostName = "host2.com" }
            };
            _organizationsDbSet.SetDbSetDataForAsync(organizations);

            var response = await _organizationService.IsOrganizationHostValidAsync("organization1@host1.com", "Organization1");

            Assert.IsTrue(response);
        }

        [Test]
        public async Task Should_Return_Requires_User_Confirmation()
        {
            var organizations = new List<Organization>
            {
                new Organization { Id = 1, ShortName = "Organization1", RequiresUserConfirmation = true },
                new Organization { Id = 2, ShortName = "Organization2" }
            };
            _organizationsDbSet.SetDbSetDataForAsync(organizations);

            var response = await _organizationService.RequiresUserConfirmationAsync(1);

            Assert.IsTrue(response);
        }

        [Test]
        public async Task Should_Get_Managing_Director()
        {
            var user = new ApplicationUser { Id = "user1", IsManagingDirector = true, FirstName = "John", LastName = "Doe", OrganizationId = 1 };

            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser> { user });

            var response = await _organizationService.GetManagingDirectorAsync(1);

            Assert.AreEqual(response.UserId, "user1");
            Assert.AreEqual(response.FullName, "John Doe");
        }

        [Test]
        public void Should_Throw_Exception_If_User_Has_No_Permission_To_Set_Managing_Director()
        {
            var userAndOrg = new UserAndOrganizationDTO { OrganizationId = 1, UserId = "user1" };
            _ = _roleService.HasRoleAsync("user1", Roles.Manager).Returns(false);

            Assert.ThrowsAsync<ValidationException>(async () => await _organizationService.SetManagingDirectorAsync("user1", userAndOrg));
        }

        [Test]
        public async Task Should_To_Set_Managing_Director()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", IsManagingDirector = false, FirstName = "John", LastName = "Doe", OrganizationId = 1 },
                new ApplicationUser { Id = "user2", IsManagingDirector = true, FirstName = "John", LastName = "Doe", OrganizationId = 1 },
                new ApplicationUser { Id = "user3", IsManagingDirector = true, FirstName = "John", LastName = "Doe", OrganizationId = 1 }
            };

            _usersDbSet.SetDbSetDataForAsync(users);

            var userAndOrg = new UserAndOrganizationDTO { OrganizationId = 1, UserId = "user1" };
            _roleService.HasRoleAsync("user1", Roles.Manager).Returns(true);

            await _organizationService.SetManagingDirectorAsync("user1", userAndOrg);

            Assert.IsTrue(users[0].IsManagingDirector);
            Assert.IsFalse(users[1].IsManagingDirector);
            Assert.IsFalse(users[2].IsManagingDirector);
        }
    }
}
