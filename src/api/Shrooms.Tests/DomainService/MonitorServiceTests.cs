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
using Shrooms.Contracts.DataTransferObjects.Models.Monitors;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Monitors;
using Shrooms.Domain.Services.Monitors;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class MonitorServiceTests
    {
        private IMonitorService _monitorService;
        private DbSet<Monitor> _monitorsDbSet;
        private IUnitOfWork2 _uow;

        [SetUp]
        public void TestInitializer()
        {
            _monitorsDbSet = Substitute.For<DbSet<Monitor>, IQueryable<Monitor>, IDbAsyncEnumerable<Monitor>>();

            _uow = Substitute.For<IUnitOfWork2>();

            _uow.GetDbSet<Monitor>().Returns(_monitorsDbSet);

            _monitorService = new MonitorService(_uow);
        }

        [Test]
        public async Task Should_Return_All_Monitors_Depending_On_Organization()
        {
            MockExternalLinks();

            var result = (await _monitorService.GetMonitorListAsync(2)).ToList();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Test1", result.First().Name);
        }

        [Test]
        public async Task Should_Return_Monitor_Details()
        {
            MockExternalLinks();

            var result = await _monitorService.GetMonitorDetailsAsync(2, 2);
            Assert.AreEqual("Test2", result.Name);
        }

        [Test]
        public void Should_Throw_Validation_Exception_While_Getting_Details()
        {
            MockExternalLinks();

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _monitorService.GetMonitorDetailsAsync(2, 3));
            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
        }

        [Test]
        public void Should_Throw_When_Creating_Monitor_With_Existing_Name()
        {
            MockExternalLinks();

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "1"
            };
            var monitor = new MonitorDto { Name = "Test1" };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _monitorService.CreateMonitorAsync(monitor, userAndOrg));
            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.DuplicatesIntolerable));
        }

        [Test]
        public async Task Should_Create_New_Monitor()
        {
            MockExternalLinks();

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "1"
            };

            var monitor = new MonitorDto { Name = "Test4" };

            await _monitorService.CreateMonitorAsync(monitor, userAndOrg);
            _monitorsDbSet.Received(1).Add(Arg.Any<Monitor>());
        }

        [Test]
        public async Task Should_Update_Monitor()
        {
            MockExternalLinks();

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "1"
            };

            var monitor = new MonitorDto { Name = "Test4", Id = 1 };

            await _monitorService.UpdateMonitorAsync(monitor, userAndOrg);
            await _uow.Received(1).SaveChangesAsync(false);
        }

        [Test]
        public void Should_Throw_When_Updating_Monitor_To_Existing_Name()
        {
            MockExternalLinks();

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "1"
            };

            var monitor = new MonitorDto { Name = "Test2", Id = 1 };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _monitorService.UpdateMonitorAsync(monitor, userAndOrg));
            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.DuplicatesIntolerable));
        }

        [Test]
        public void Should_Throw_When_Updating_Not_Existing_Monitor()
        {
            MockExternalLinks();

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "1"
            };

            var monitor = new MonitorDto { Name = "Test2", Id = 5 };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _monitorService.UpdateMonitorAsync(monitor, userAndOrg));
            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
        }

        private void MockExternalLinks()
        {
            var monitors = new List<Monitor>
            {
                new()
                {
                    Id = 1,
                    Name = "Test1",
                    OrganizationId = 2
                },
                new()
                {
                    Id = 2,
                    Name = "Test2",
                    OrganizationId = 2
                },
                new()
                {
                    Id = 3,
                    Name = "Test3",
                    OrganizationId = 1
                }
            }.AsQueryable();

            _monitorsDbSet.SetDbSetDataForAsync(monitors);
        }
    }
}
