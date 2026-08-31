using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
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
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
        private ISystemClock _systemClock;

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
            _systemClock = Substitute.For<ISystemClock>();

            var eventParticipationService = Substitute.For<IEventParticipationService>();
            var eventUtilitiesService = Substitute.For<IEventUtilitiesService>();
            var markdownConverter = Substitute.For<IMarkdownConverter>();
            var eventQuestionWriter = Substitute.For<IEventQuestionWriter>();

            _eventService = new EventService(
                _uow,
                _permissionService,
                eventUtilitiesService,
                _eventValidationService,
                eventParticipationService,
                _wallService,
                markdownConverter,
                _officeMapService,
                _systemClock,
                eventQuestionWriter);
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

            ClassicAssert.AreEqual(result.Name, @event.Name);
            ClassicAssert.AreEqual(result.HostUserId, @event.ResponsibleUser.Id);
        }

        [Test]
        public async Task Should_Return_The_Question_Tree_For_Editing()
        {
            var eventId = MockEventForEditingWithQuestions();
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "testUser1" };

            var result = await _eventService.GetEventForEditingAsync(eventId, userOrg);

            var questions = result.Questions.ToList();
            Assert.That(questions, Has.Count.EqualTo(2));
            Assert.That(questions[0].Id, Is.EqualTo(5));
            Assert.That(questions[0].Title, Is.EqualTo("Pick your dish"));
            Assert.That(questions[0].SelectType, Is.EqualTo(EventQuestionSelectType.Single));
            Assert.That(questions[0].IsRequired, Is.True);
            Assert.That(questions[0].ShowIfOptionId, Is.Null);
            Assert.That(questions[0].Options.Select(o => o.Id), Is.EqualTo(new int?[] { 90, 91 }));
            Assert.That(questions[0].Options[0].Name, Is.EqualTo("Pasta"));
            Assert.That(questions[1].ShowIfOptionId, Is.EqualTo(91));
        }

        // Regression test: MapToEventEditDetailsDto used to project every EventOption into the
        // flat Options list, including ones owned by a question. That leaked question options
        // into the edit payload's editedOptions on the client. Only legacy (QuestionId == null)
        // options belong in Options; question-owned options are exposed solely under Questions.
        [Test]
        public async Task Should_Only_Return_Legacy_Options_In_Flat_Options_List_When_Editing()
        {
            var eventId = MockEventForEditingWithLegacyAndQuestionOptions();
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "testUser1" };

            var result = await _eventService.GetEventForEditingAsync(eventId, userOrg);

            var flatOptions = result.Options.ToList();
            Assert.That(flatOptions, Has.Count.EqualTo(1));
            Assert.That(flatOptions[0].Id, Is.EqualTo(80));
            Assert.That(flatOptions[0].Option, Is.EqualTo("Legacy option"));

            var questions = result.Questions.ToList();
            Assert.That(questions, Has.Count.EqualTo(1));
            Assert.That(questions[0].Options.Select(o => o.Id), Is.EquivalentTo(new int?[] { 90, 91 }));
        }

        // Regression test: MapToEventDetailsDto projected every EventOption into the flat Options
        // list, so GET /Events/Details served question-owned options as if they were legacy food
        // options. Only legacy (QuestionId == null) options belong there — question-owned options
        // reach the client through the question tree.
        [Test]
        public async Task Should_Only_Return_Legacy_Options_In_Flat_Options_List_In_Details()
        {
            var eventId = MockEventDetailsWithLegacyAndQuestionOptions();
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "testUser1" };

            var result = await _eventService.GetEventDetailsAsync(eventId, userOrg);

            var flatOptions = result.Options.ToList();
            Assert.That(flatOptions, Has.Count.EqualTo(1));
            Assert.That(flatOptions[0].Id, Is.EqualTo(80));
            Assert.That(flatOptions[0].Name, Is.EqualTo("Legacy option"));
        }

        private Guid MockEventDetailsWithLegacyAndQuestionOptions()
        {
            var eventId = Guid.NewGuid();
            var responsibleUser = new ApplicationUser
            {
                Id = "responsibleUser1",
                FirstName = "user1f",
                LastName = "user1l"
            };

            var options = new List<EventOption>
            {
                new EventOption { Id = 80, EventId = eventId, Option = "Legacy option", QuestionId = null, Order = 0, EventParticipants = new List<EventParticipant>() },
                new EventOption { Id = 90, EventId = eventId, Option = "Pasta", QuestionId = 5, Order = 0, EventParticipants = new List<EventParticipant>() },
                new EventOption { Id = 91, EventId = eventId, Option = "Pizza", QuestionId = 5, Order = 1, EventParticipants = new List<EventParticipant>() }
            };

            var events = new List<Event>
            {
                new Event
                {
                    Id = eventId,
                    OrganizationId = 2,
                    Offices = "[\"1\"]",
                    MaxChoices = 1,
                    ResponsibleUser = responsibleUser,
                    ResponsibleUserId = responsibleUser.Id,
                    EventOptions = options,
                    EventParticipants = new List<EventParticipant>()
                }
            };

            _officeDbSet.SetDbSetDataForAsync(new List<Office> { new Office { Id = 1, Name = "Office", OrganizationId = 2 } });
            _eventsDbSet.SetDbSetDataForAsync(events.AsQueryable());

            return eventId;
        }

        private Guid MockEventForEditingWithLegacyAndQuestionOptions()
        {
            var eventId = Guid.NewGuid();
            var responsibleUser = new ApplicationUser
            {
                Id = "responsibleUser1",
                FirstName = "user1f",
                LastName = "user1l"
            };

            var legacyOption = new EventOption { Id = 80, EventId = eventId, Option = "Legacy option", QuestionId = null, Order = 0 };

            var questionOptions = new List<EventOption>
            {
                new EventOption { Id = 90, EventId = eventId, Option = "Pasta", QuestionId = 5, Order = 0 },
                new EventOption { Id = 91, EventId = eventId, Option = "Pizza", QuestionId = 5, Order = 1 }
            };

            var questions = new List<EventQuestion>
            {
                new EventQuestion
                {
                    Id = 5,
                    EventId = eventId,
                    Title = "Pick your dish",
                    Order = 0,
                    SelectType = EventQuestionSelectType.Single,
                    IsRequired = true,
                    ShowIfOptionId = null,
                    Options = questionOptions
                }
            };

            var events = new List<Event>
            {
                new Event
                {
                    Id = eventId,
                    OrganizationId = 2,
                    ResponsibleUser = responsibleUser,
                    ResponsibleUserId = responsibleUser.Id,
                    Reminders = new List<EventReminder>(),
                    EventOptions = new List<EventOption> { legacyOption, questionOptions[0], questionOptions[1] },
                    EventQuestions = questions
                }
            };

            _eventsDbSet.SetDbSetDataForAsync(events.AsQueryable());

            return eventId;
        }

        private Guid MockEventForEditingWithQuestions()
        {
            var eventId = Guid.NewGuid();
            var responsibleUser = new ApplicationUser
            {
                Id = "responsibleUser1",
                FirstName = "user1f",
                LastName = "user1l"
            };

            // Seeded out of order (91 before 90) so the Options OrderBy is genuinely exercised.
            var dishOptions = new List<EventOption>
            {
                new EventOption { Id = 91, EventId = eventId, Option = "Pizza", QuestionId = 5, Order = 1 },
                new EventOption { Id = 90, EventId = eventId, Option = "Pasta", QuestionId = 5, Order = 0 }
            };

            var pizzaOptions = new List<EventOption>
            {
                new EventOption { Id = 92, EventId = eventId, Option = "Margherita", QuestionId = 6, Order = 0 }
            };

            // Seeded out of order (question 6 before question 5) so the Questions OrderBy is
            // genuinely exercised.
            var questions = new List<EventQuestion>
            {
                new EventQuestion
                {
                    Id = 6,
                    EventId = eventId,
                    Title = "Which pizza?",
                    Order = 1,
                    SelectType = EventQuestionSelectType.Single,
                    IsRequired = true,
                    ShowIfOptionId = 91,
                    Options = pizzaOptions
                },
                new EventQuestion
                {
                    Id = 5,
                    EventId = eventId,
                    Title = "Pick your dish",
                    Order = 0,
                    SelectType = EventQuestionSelectType.Single,
                    IsRequired = true,
                    ShowIfOptionId = null,
                    Options = dishOptions
                }
            };

            var events = new List<Event>
            {
                new Event
                {
                    Id = eventId,
                    OrganizationId = 2,
                    ResponsibleUser = responsibleUser,
                    ResponsibleUserId = responsibleUser.Id,
                    Reminders = new List<EventReminder>(),
                    EventOptions = new List<EventOption> { dishOptions[0], dishOptions[1], pizzaOptions[0] },
                    EventQuestions = questions
                }
            };

            _eventsDbSet.SetDbSetDataForAsync(events.AsQueryable());

            return eventId;
        }
    }
}
