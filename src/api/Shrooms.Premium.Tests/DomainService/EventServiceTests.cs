using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;
using Shrooms.Premium.Domain.Services.Events;
using Shrooms.Premium.Domain.Services.Events.Participation;
using Shrooms.Premium.Domain.Services.Events.Utilities;
using Shrooms.Premium.Domain.Services.OfficeMap;
using Shrooms.Tests.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Shrooms.Premium.Tests.DomainService
{
    [TestFixture]
    public class EventServiceTests
    {
        private DbSet<Event> _eventsDbSet;
        private DbSet<Office> _officeDbSet;
        private DbSet<ApplicationUser> _usersDbSet;

        private IUnitOfWork2 _uow;
        private IWallService _wallService;
        private IEventService _eventService;
        private IPermissionService _permissionService;
        private IOfficeMapService _officeMapService;
        private IEventValidationService _eventValidationService;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();

            _eventsDbSet = _uow.MockDbSetForAsync<Event>();
            _officeDbSet = _uow.MockDbSetForAsync<Office>();
            _usersDbSet = _uow.MockDbSetForAsync<ApplicationUser>();

            _permissionService = Substitute.For<IPermissionService>();
            _wallService = Substitute.For<IWallService>();
            _eventValidationService = Substitute.For<IEventValidationService>();
            _officeMapService = Substitute.For<IOfficeMapService>();

            var eventParticipationService = Substitute.For<IEventParticipationService>();
            var eventUtilitiesService = Substitute.For<IEventUtilitiesService>();

            _eventService = new EventService(_uow,
                _permissionService,
                eventUtilitiesService,
                _eventValidationService,
                eventParticipationService,
                _wallService,
                _officeMapService);
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task Should_Return_Event_Report_Details(int organizationId)
        {
            // Arrange
            _officeMapService.GetOfficesCountAsync()
                .Returns(1);

            var office = new Office
            {
                Id = 1,
                Name = "Office",
                OrganizationId = organizationId
            };

            var responsibleUserId = Guid.NewGuid().ToString();
            var responsibleUser = new ApplicationUser
            {
                Id = responsibleUserId,
                FirstName = "Arnold",
                LastName = "Something",
                OrganizationId = organizationId
            };

            const string offices = "[\"1\"]";

            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Name = "Awesome event",
                Id = eventId,
                Place = "Place",
                Offices = offices,
                OrganizationId = organizationId,
                ResponsibleUser = responsibleUser,
                ResponsibleUserId = responsibleUserId
            };

            _officeDbSet.SetDbSetDataForAsync(new List<Office> { office });
            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event });
            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser> { responsibleUser });

            // Act
            var result = await _eventService.GetReportEventDetailsAsync(eventId, new UserAndOrganizationDto { OrganizationId = organizationId });

            // Assert
            _eventValidationService
                .Received(1)
                .CheckIfEventExists(Arg.Any<EventReportDetailsDto>());

            Assert.AreEqual(result.Name, @event.Name);
            Assert.AreEqual(result.HostUserId, @event.ResponsibleUser.Id);
        }
    }
}
