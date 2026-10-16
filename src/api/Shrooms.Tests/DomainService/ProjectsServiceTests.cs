using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Projects;
using Shrooms.Domain.Services.Wall;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class ProjectsServiceTests
    {
        private DbSet<ApplicationUser> _usersDbSet;
        private IProjectsService _projectsService;

        [SetUp]
        public void Init()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            _usersDbSet = uow.MockDbSetForAsync<ApplicationUser>();

            _projectsService = new ProjectsService(uow, Substitute.For<IWallService>(), Substitute.For<IPermissionService>());
        }

        [Test]
        public async Task ValidateManagerId_Allows_Managing_Director_To_Report_To_Own_Subordinate()
        {
            MockUsers(
                new ApplicationUser { Id = "director", IsManagingDirector = true },
                new ApplicationUser { Id = "subordinate", ManagerId = "director" });

            var result = await _projectsService.ValidateManagerIdAsync("director", "subordinate");

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task ValidateManagerId_Does_Not_Allow_Managing_Director_To_Manage_Themselves()
        {
            MockUsers(new ApplicationUser { Id = "director", IsManagingDirector = true });

            var result = await _projectsService.ValidateManagerIdAsync("director", "director");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ValidateManagerId_Allows_Chain_That_Reaches_Managing_Director_With_A_Manager()
        {
            MockUsers(
                new ApplicationUser { Id = "director", IsManagingDirector = true, ManagerId = "user" },
                new ApplicationUser { Id = "lead", ManagerId = "director" },
                new ApplicationUser { Id = "user", ManagerId = "director" });

            var result = await _projectsService.ValidateManagerIdAsync("user", "lead");

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task ValidateManagerId_Does_Not_Allow_Cycle_Below_Managing_Director()
        {
            MockUsers(
                new ApplicationUser { Id = "director", IsManagingDirector = true },
                new ApplicationUser { Id = "lead", ManagerId = "director" },
                new ApplicationUser { Id = "member", ManagerId = "lead" });

            var result = await _projectsService.ValidateManagerIdAsync("lead", "member");

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ValidateManagerId_Does_Not_Loop_Forever_On_Existing_Cycle()
        {
            MockUsers(
                new ApplicationUser { Id = "first", ManagerId = "second" },
                new ApplicationUser { Id = "second", ManagerId = "first" },
                new ApplicationUser { Id = "user" });

            var result = await _projectsService.ValidateManagerIdAsync("user", "first");

            Assert.That(result, Is.False);
        }

        private void MockUsers(params ApplicationUser[] users)
        {
            foreach (var user in users)
            {
                _usersDbSet.FindAsync(user.Id).Returns(user);
            }
        }
    }
}
