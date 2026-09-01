using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Premium.Constants;
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
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Shrooms.Premium.Tests.Controllers.WebApi
{
    [TestFixture]
    public class EventControllerTests
    {
        private EventController _eventController;

        private IEventService _eventService;
        private IEventListingService _eventListingService;
        private IEventParticipationService _eventParticipationService;

        [SetUp]
        public void TestInitializer()
        {
            var mapper = ModelMapper.Create();

            _eventService = Substitute.For<IEventService>();
            _eventListingService = Substitute.For<IEventListingService>();
            _eventParticipationService = Substitute.For<IEventParticipationService>();

            var eventUtilitiesService = Substitute.For<IEventUtilitiesService>();
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
                _eventParticipationService,
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

            // Assert
            ClassicAssert.AreEqual(HttpStatusCode.OK, httpActionResult.GetStatusCode());
        }

        [Test]
        public async Task GetReportEventDetails_Should_Return_Bad_Request_When_EventException_Is_Thrown()
        {
            // Arrange
            _eventService.GetReportEventDetailsAsync(Arg.Any<Guid>(), Arg.Any<UserAndOrganizationDto>())
                .ThrowsAsync(new EventException("Error"));

            var eventId = Guid.NewGuid();

            // Act
            var httpActionResult = await _eventController.GetReportEventDetails(eventId);

            // Assert
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, httpActionResult.GetStatusCode());
        }

        [Test]
        public async Task GetEventsByTitle_Should_Return_Ok()
        {
            // Arrange
            var reportArgsViewModel = new EventReportListingArgsViewModel();

            // Act
            var httpActionResult = await _eventController.GetEventsByTitle(reportArgsViewModel);

            // Assert
            ClassicAssert.AreEqual(HttpStatusCode.OK, httpActionResult.GetStatusCode());
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

            // Assert
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, httpActionResult.GetStatusCode());
        }

        [Test]
        public async Task GetEventsByTitle_Should_Return_Bad_Request_When_EventException_Is_Thrown()
        {
            // Arrange
            _eventListingService.GetNotStartedEventsFilteredByTitleAsync(
                Arg.Any<EventReportListingArgsDto>(),
                Arg.Any<UserAndOrganizationDto>())
                .ThrowsAsync(new EventException("Error"));

            var reportArgsViewModel = new EventReportListingArgsViewModel();

            // Act
            var httpActionResult = await _eventController.GetEventsByTitle(reportArgsViewModel);

            // Assert
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, httpActionResult.GetStatusCode());
        }

        [Test]
        public async Task GetPagedReportParticipants_Should_Return_Ok()
        {
            // Arrange
            var reportArgsViewModel = new EventParticipantsReportListingArgsViewModel();

            // Act
            var httpActionResult = await _eventController.GetPagedReportParticipants(reportArgsViewModel);

            // Assert
            ClassicAssert.AreEqual(HttpStatusCode.OK, httpActionResult.GetStatusCode());
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

            // Assert
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, httpActionResult.GetStatusCode());
        }

        [Test]
        public async Task GetPagedReportParticipants_Should_Return_Bad_Request_When_EventException_Is_Thrown()
        {
            // Arrange
            _eventListingService.GetReportParticipantsAsync(
                Arg.Any<EventParticipantsReportListingArgsDto>(),
                Arg.Any<UserAndOrganizationDto>())
                .ThrowsAsync(new EventException("Error"));

            var reportArgsViewModel = new EventParticipantsReportListingArgsViewModel();

            // Act
            var httpActionResult = await _eventController.GetPagedReportParticipants(reportArgsViewModel);

            // Assert
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, httpActionResult.GetStatusCode());
        }

        [Test]
        public async Task GetPagedVisitedReportEvents_Should_Return_Bad_Request_When_EventException_Is_Thrown()
        {
            // Arrange
            var visitedArgsViewModel = new EventParticipantVisitedEventsListingArgsViewModel();

            _eventController.Validate(visitedArgsViewModel);

            // Act
            var httpActionResult = await _eventController.GetPagedVisitedReportEvents(visitedArgsViewModel);

            // Assert
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, httpActionResult.GetStatusCode());
        }

        [Test]
        public async Task GetPagedVisitedReportEvents_Should_Return_Bad_Request_When_ModelState_Is_Invalid()
        {
            // Arrange
            _eventListingService.GetEventParticipantVisitedReportEventsAsync(
                Arg.Any<EventParticipantVisitedEventsListingArgsDto>(),
                Arg.Any<UserAndOrganizationDto>())
                .ThrowsAsync(new EventException("Error"));

            var visitedArgsViewModel = new EventParticipantVisitedEventsListingArgsViewModel();

            // Act
            var httpActionResult = await _eventController.GetPagedVisitedReportEvents(visitedArgsViewModel);

            // Assert
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, httpActionResult.GetStatusCode());
        }

        [Test]
        public async Task GetPagedVisitedReportEvents_Should_Return_Ok()
        {
            // Arrange
            var visitedArgsViewModel = new EventParticipantVisitedEventsListingArgsViewModel();

            // Act
            var httpActionResult = await _eventController.GetPagedVisitedReportEvents(visitedArgsViewModel);

            // Assert
            ClassicAssert.AreEqual(HttpStatusCode.OK, httpActionResult.GetStatusCode());
        }

        [Test]
        public async Task Join_Should_Return_The_Offending_Questions_When_Answers_Are_Invalid()
        {
            _eventParticipationService
                .JoinAsync(Arg.Any<EventJoinDto>())
                .Returns(Task.FromException(new EventAnswersInvalidException(new List<EventAnswerErrorDto>
                {
                    new EventAnswerErrorDto { QuestionId = 12, Reason = EventAnswerErrorReason.RequiredAnswerMissing }
                })));

            var result = await _eventController.Join(new EventJoinViewModel { EventId = Guid.NewGuid() });

            Assert.That(result.GetStatusCode(), Is.EqualTo(HttpStatusCode.BadRequest));
            var body = result.GetContent<EventAnswersInvalidViewModel>();
            Assert.That(body.Code, Is.EqualTo(PremiumErrorCodes.EventAnswersInvalid));
            Assert.That(body.Errors, Has.Count.EqualTo(1));
            Assert.That(body.Errors[0].QuestionId, Is.EqualTo(12));
            Assert.That(body.Errors[0].Reason, Is.EqualTo(EventAnswerErrorReason.RequiredAnswerMissing));
        }

        [Test]
        public void Should_Serialize_Answer_Error_Reason_As_A_String_For_The_Client()
        {
            var viewModel = new EventAnswersInvalidViewModel
            {
                Code = "EventAnswersInvalid",
                Errors = new List<EventAnswerErrorViewModel>
                {
                    new EventAnswerErrorViewModel
                    {
                        QuestionId = 12,
                        Reason = EventAnswerErrorReason.RequiredAnswerMissing
                    }
                }
            };

            var json = JsonConvert.SerializeObject(viewModel, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            Assert.That(json, Does.Contain("\"reason\":\"RequiredAnswerMissing\""));
            Assert.That(json, Does.Contain("\"questionId\":12"));
        }

        [Test]
        public async Task UpdateAttendStatus_Should_Return_The_Offending_Questions_When_Answers_Are_Invalid()
        {
            _eventParticipationService
                .UpdateAttendStatusAsync(Arg.Any<UpdateAttendStatusDto>())
                .Returns(Task.FromException(new EventAnswersInvalidException(new List<EventAnswerErrorDto>
                {
                    new EventAnswerErrorDto { QuestionId = 12, Reason = EventAnswerErrorReason.RequiredAnswerMissing }
                })));

            var result = await _eventController.UpdateAttendStatus(new UpdateAttendStatusViewModel { EventId = Guid.NewGuid() });

            Assert.That(result.GetStatusCode(), Is.EqualTo(HttpStatusCode.BadRequest));
            var body = result.GetContent<EventAnswersInvalidViewModel>();
            Assert.That(body.Code, Is.EqualTo(PremiumErrorCodes.EventAnswersInvalid));
            Assert.That(body.Errors, Has.Count.EqualTo(1));
            Assert.That(body.Errors[0].QuestionId, Is.EqualTo(12));
            Assert.That(body.Errors[0].Reason, Is.EqualTo(EventAnswerErrorReason.RequiredAnswerMissing));
        }

    }
}
