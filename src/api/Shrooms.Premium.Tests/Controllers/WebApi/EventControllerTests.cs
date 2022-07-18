using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
using Shrooms.Premium.Domain.Services.Events;
using Shrooms.Premium.Domain.Services.Events.Calendar;
using Shrooms.Premium.Domain.Services.Events.Export;
using Shrooms.Premium.Domain.Services.Events.List;
using Shrooms.Premium.Domain.Services.Events.Participation;
using Shrooms.Premium.Domain.Services.Events.Utilities;
using Shrooms.Premium.Domain.Services.OfficeMap;
using Shrooms.Premium.Presentation.Api.Controllers;
using Shrooms.Premium.Presentation.WebViewModels.Events;
using Shrooms.Premium.Tests.ModelMappings;
using Shrooms.Tests.Extensions;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;

namespace Shrooms.Premium.Tests.Controllers.WebApi
{
    [TestFixture]
    public class EventControllerTests
    {
        private EventController _eventController;

        private IEventService _eventService;
        private IEventListingService _eventListingService;

        [SetUp]
        public void TestInitializer()
        {
            var mapper = ModelMapper.Create();

            _eventService = Substitute.For<IEventService>();
            _eventListingService = Substitute.For<IEventListingService>();

            var eventUtilitiesService = Substitute.For<IEventUtilitiesService>();
            var eventParticipationService = Substitute.For<IEventParticipationService>();
            var eventCalendarService = Substitute.For<IEventCalendarService>();
            var eventExportService = Substitute.For<IEventExportService>();
            var postService = Substitute.For<IPostService>();
            var officeMapService = Substitute.For<IOfficeMapService>();
            var asyncRunner = Substitute.For<IAsyncRunner>();

            _eventController = new EventController(
                mapper,
                _eventService,
                _eventListingService,
                eventUtilitiesService,
                eventParticipationService,
                eventCalendarService,
                eventExportService,
                postService,
                officeMapService,
                asyncRunner);
            _eventController.SetUpControllerForTesting();
        }

        [Test]
        public async Task GetReportEventDetails_Should_Return_Ok()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            // Act
            var httpActionResult = await _eventController.GetReportEventDetails(eventId);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetReportEventDetails_Should_Return_Bad_Request_When_EventException_Is_Thrown()
        {
            // Arrange
            _eventService.GetReportEventDetailsAsync(Arg.Any<Guid>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new EventException("Error"));

            var eventId = Guid.NewGuid();

            // Act
            var httpActionResult = await _eventController.GetReportEventDetails(eventId);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task GetEventsByTitle_Should_Return_Ok()
        {
            // Arrange
            var reportArgsViewModel = new EventReportListingArgsViewModel();

            // Act
            var httpActionResult = await _eventController.GetEventsByTitle(reportArgsViewModel);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetEventsByTitle_Should_Return_Bad_Request_When_ModelState_Is_Invalid()
        {
            // Arrange
            var reportArgsViewModel = new EventReportListingArgsViewModel
            {
                Page = int.MinValue
            };

            // Act
            _eventController.Validate(reportArgsViewModel);

            var httpActionResult = await _eventController.GetEventsByTitle(reportArgsViewModel);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task GetEventsByTitle_Should_Return_Bad_Request_When_EventException_Is_Thrown()
        {
            // Arrange
            _eventListingService.GetNotStartedEventsFilteredByTitleAsync(
                Arg.Any<EventReportListingArgsDto>(),
                Arg.Any<UserAndOrganizationDto>())
                .Throws(new EventException("Error"));

            var reportArgsViewModel = new EventReportListingArgsViewModel();

            // Act
            var httpActionResult = await _eventController.GetEventsByTitle(reportArgsViewModel);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task GetPagedReportParticipants_Should_Return_Ok()
        {
            // Arrange
            var reportArgsViewModel = new EventParticipantsReportListingArgsViewModel();

            // Act
            var httpActionResult = await _eventController.GetPagedReportParticipants(reportArgsViewModel);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetPagedReportParticipants_Should_Return_Bad_Request_When_ModelState_Is_Invalid()
        {
            // Arrange
            var reportArgsViewModel = new EventParticipantsReportListingArgsViewModel
            {
                Page = int.MinValue
            };

            // Act
            _eventController.Validate(reportArgsViewModel);

            var httpActionResult = await _eventController.GetPagedReportParticipants(reportArgsViewModel);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task GetPagedReportParticipants_Should_Return_Bad_Request_When_EventException_Is_Thrown()
        {
            // Arrange
            _eventListingService.GetReportParticipantsAsync(
                Arg.Any<EventParticipantsReportListingArgsDto>(),
                Arg.Any<UserAndOrganizationDto>())
                .Throws(new EventException("Error"));

            var reportArgsViewModel = new EventParticipantsReportListingArgsViewModel();

            // Act
            var httpActionResult = await _eventController.GetPagedReportParticipants(reportArgsViewModel);
            var response = await httpActionResult.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
