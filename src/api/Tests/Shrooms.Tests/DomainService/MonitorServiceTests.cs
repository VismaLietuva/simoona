using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Constants.ErrorCodes;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Monitors;
using Shrooms.Domain.Services.Monitors;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models.Monitors;
using Shrooms.Host.Contracts.DAL;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.UnitTests.DomainService
{
    [TestFixture]
    public class MonitorServiceTests
    {
        private IMonitorService _monitorService;
        private IDbSet<Monitor> _monitorsDbSet;
        private IUnitOfWork2 _uow;

        [SetUp]
        public void TestInitializer()
        {
            _monitorsDbSet = Substitute.For<IDbSet<Monitor>>();

            _uow = Substitute.For<IUnitOfWork2>();

            _uow.GetDbSet<Monitor>().Returns(_monitorsDbSet);

            _monitorService = new MonitorService(_uow);
        }

        [Test]
        public void Should_Return_All_Monitors_Depending_On_Organization()
        {
            MockExternalLinks();

            var result = _monitorService.GetMonitorList(2).ToList();
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Test1", result.First().Name);
        }

        [Test]
        public void Should_Return_Monitor_Details()
        {
            MockExternalLinks();

            var result = _monitorService.GetMonitorDetails(2, 2);
            Assert.AreEqual("Test2", result.Name);
        }

        [Test]
        public void Should_Throw_Validation_Exception_While_Getting_Details()
        {
            MockExternalLinks();

            var ex = Assert.Throws<ValidationException>(() => _monitorService.GetMonitorDetails(2, 3));
            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
        }

        [Test]
        public void Should_Throw_When_Creating_Monitor_With_Existing_Name()
        {
            MockExternalLinks();

            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 2,
                UserId = "1"
            };
            var monitor = new MonitorDTO() { Name = "Test1" };

            var ex = Assert.Throws<ValidationException>(() => _monitorService.CreateMonitor(monitor, userAndOrg));
            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.DuplicatesIntolerable));
        }

        [Test]
        public void Should_Create_New_Monitor()
        {
            MockExternalLinks();

            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 2,
                UserId = "1"
            };

            var monitor = new MonitorDTO() { Name = "Test4" };

            _monitorService.CreateMonitor(monitor, userAndOrg);
            _monitorsDbSet.Received(1).Add(Arg.Any<Monitor>());
        }

        [Test]
        public void Should_Update_Monitor()
        {
            MockExternalLinks();

            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 2,
                UserId = "1"
            };

            var monitor = new MonitorDTO() { Name = "Test4", Id = 1 };

            _monitorService.UpdateMonitor(monitor, userAndOrg);
            _uow.Received(1).SaveChanges(false);
        }

        [Test]
        public void Should_Throw_When_Updating_Monitor_To_Existing_Name()
        {
            MockExternalLinks();

            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 2,
                UserId = "1"
            };

            var monitor = new MonitorDTO() { Name = "Test2", Id = 1 };

            var ex = Assert.Throws<ValidationException>(() => _monitorService.UpdateMonitor(monitor, userAndOrg));
            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.DuplicatesIntolerable));
        }

        [Test]
        public void Should_Throw_When_Updating_Not_Existing_Monitor()
        {
            MockExternalLinks();

            var userAndOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 2,
                UserId = "1"
            };

            var monitor = new MonitorDTO() { Name = "Test2", Id = 5 };

            var ex = Assert.Throws<ValidationException>(() => _monitorService.UpdateMonitor(monitor, userAndOrg));
            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
        }

        private void MockExternalLinks()
        {
            var monitors = new List<Monitor>()
            {
                new Monitor
                {
                    Id = 1,
                    Name = "Test1",
                    OrganizationId = 2
                },
                new Monitor
                {
                    Id = 2,
                    Name = "Test2",
                    OrganizationId = 2
                },
                new Monitor
                {
                    Id = 3,
                    Name = "Test3",
                    OrganizationId = 1
                }
            }.AsQueryable();

            _monitorsDbSet.SetDbSetData(monitors);
        }
    }
}
