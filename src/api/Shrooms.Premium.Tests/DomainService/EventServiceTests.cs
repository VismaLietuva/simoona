using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.Premium.Constants;
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
using System.Threading;
using System.Threading.Tasks;

namespace Shrooms.Premium.Tests.DomainService
{
    [TestFixture]
    public class EventServiceTests
    {
        private DbSet<Event> _eventsDbSet;
        private DbSet<Office> _officeDbSet;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<EventParticipant> _eventParticipantsDbSet;
        private DbSet<EventOption> _eventOptionsDbSet;
        private DbSet<EventQuestion> _eventQuestionsDbSet;

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
            _eventParticipantsDbSet = _uow.MockDbSetForAsync<EventParticipant>();
            _eventOptionsDbSet = _uow.MockDbSetForAsync<EventOption>();
            _eventQuestionsDbSet = _uow.MockDbSetForAsync<EventQuestion>();

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
            _officeMapService.GetOfficesCountAsync(Arg.Any<CancellationToken>())
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
        public void GetEventDetailsAsync_CancelledToken_StopsTheQuery()
        {
            Assert.That(
                async () => await _eventService.GetEventDetailsAsync(
                    Guid.NewGuid(),
                    new UserAndOrganizationDto { OrganizationId = 2 },
                    new CancellationToken(true)),
                Throws.InstanceOf<OperationCanceledException>());
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

        [Test]
        public async Task Should_Return_Host_Picture_In_Details_Even_When_Host_Is_Not_A_Participant()
        {
            var eventId = MockEventDetailsWithLegacyAndQuestionOptions();
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "testUser1" };

            var result = await _eventService.GetEventDetailsAsync(eventId, userOrg);

            Assert.That(result.HostUserImageName, Is.EqualTo("host-picture.png"));
        }

        [Test]
        public async Task Should_Return_The_Question_Tree_With_Its_Answers_In_Details()
        {
            var eventId = MockEventDetailsWithAnsweredQuestion(canSeeParticipants: true);
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "testUser1" };

            var result = await _eventService.GetEventDetailsAsync(eventId, userOrg);

            var questions = result.Questions.ToList();
            Assert.That(questions, Has.Count.EqualTo(2));
            Assert.That(questions[0].Id, Is.EqualTo(5));
            Assert.That(questions[0].Title, Is.EqualTo("Pick your dish"));
            Assert.That(questions[0].SelectType, Is.EqualTo(EventQuestionSelectType.Single));
            Assert.That(questions[0].IsRequired, Is.True);
            Assert.That(questions[0].ShowIfOptionId, Is.Null);
            Assert.That(questions[1].ShowIfOptionId, Is.EqualTo(91));

            var dish = questions[0].Options.ToList();
            Assert.That(dish.Select(option => option.Id), Is.EqualTo(new[] { 90, 91 }));
            Assert.That(dish[0].Name, Is.EqualTo("Pasta"));
            Assert.That(dish[0].Participants.Select(p => p.UserId), Is.EquivalentTo(new[] { "testUser1", "otherUser" }));
            Assert.That(dish[1].Participants, Is.Empty);
        }

        [Test]
        public async Task Should_Return_The_Callers_Own_Selection_In_Details()
        {
            // The per-option Participants lists only name people who are Going, so this is the only
            // place a Maybe participant can learn what they already answered — which is exactly
            // what a status change back to Going re-asserts.
            var eventId = MockEventDetailsWithAnsweredQuestion(canSeeParticipants: false);
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "testUser1" };

            var result = await _eventService.GetEventDetailsAsync(eventId, userOrg);

            Assert.That(result.MyChosenOptions, Is.EquivalentTo(new[] { 90 }));
        }

        [Test]
        public async Task Should_Hide_Other_Peoples_Answers_From_A_Caller_Who_Cannot_See_Participants()
        {
            var eventId = MockEventDetailsWithAnsweredQuestion(canSeeParticipants: false);
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "testUser1" };

            var result = await _eventService.GetEventDetailsAsync(eventId, userOrg);

            var answered = result.Questions.First().Options.First();
            Assert.That(answered.Participants.Select(p => p.UserId), Is.EqualTo(new[] { "testUser1" }),
                "an answer picked by someone else must not name them");
        }

        [Test]
        public async Task Should_Hide_Participants_On_A_Legacy_Option_The_Caller_Did_Not_Pick()
        {
            var eventId = MockEventDetailsWithAnsweredQuestion(canSeeParticipants: false);
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "testUser1" };

            var result = await _eventService.GetEventDetailsAsync(eventId, userOrg);

            var soup = result.Options.Single(option => option.Name == "Soup");
            Assert.That(soup.Participants, Is.Empty,
                "only the caller's own picks used to be rewritten, so every other option leaked its participants");
        }

        [Test]
        public async Task Should_Count_Every_Participant_Even_When_The_Caller_Sees_Only_Their_Own_Row()
        {
            var eventId = MockEventDetailsWithAnsweredQuestion(canSeeParticipants: false);
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "testUser1" };

            var result = await _eventService.GetEventDetailsAsync(eventId, userOrg);

            Assert.That(result.Participants.Select(p => p.UserId), Is.EqualTo(new[] { "testUser1" }));
            Assert.That(result.GoingCount, Is.EqualTo(2), "counts must not shrink with the trimmed participant list");
            Assert.That(result.MaybeGoingCount, Is.EqualTo(1));
            Assert.That(result.VirtuallyGoingCount, Is.EqualTo(0));
            Assert.That(result.NotGoingCount, Is.EqualTo(0));
            Assert.That(result.IsFull, Is.True, "two going with room for two");
            Assert.That(result.ParticipatingStatus, Is.EqualTo(AttendingStatus.Attending));
        }

        [Test]
        public async Task Should_Return_Every_Participant_To_A_Caller_Who_Can_See_Them()
        {
            var eventId = MockEventDetailsWithAnsweredQuestion(canSeeParticipants: true);
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "testUser1" };

            var result = await _eventService.GetEventDetailsAsync(eventId, userOrg);

            Assert.That(result.Participants.Select(p => p.UserId), Is.EquivalentTo(new[] { "testUser1", "otherUser", "maybeUser" }));
            Assert.That(result.Options.Single(option => option.Name == "Soup").Participants.Select(p => p.UserId), Is.EqualTo(new[] { "otherUser" }));
        }

        [Test]
        public async Task Should_Return_The_Status_And_Answers_Of_A_Maybe_Caller()
        {
            var eventId = MockEventDetailsWithAnsweredQuestion(canSeeParticipants: false);
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "maybeUser" };

            var result = await _eventService.GetEventDetailsAsync(eventId, userOrg);

            Assert.That(result.ParticipatingStatus, Is.EqualTo(AttendingStatus.MaybeAttending));
            Assert.That(result.MyChosenOptions, Is.EquivalentTo(new[] { 90 }));
            Assert.That(result.Participants.Select(p => p.UserId), Is.EqualTo(new[] { "maybeUser" }));
            Assert.That(result.Questions.First().Options.First().Participants, Is.Empty,
                "option lists name only people who are going");
        }

        [Test]
        public async Task Should_Return_Idle_And_No_Participants_To_A_Caller_Who_Has_Not_Responded()
        {
            var eventId = MockEventDetailsWithAnsweredQuestion(canSeeParticipants: false);
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "stranger" };

            var result = await _eventService.GetEventDetailsAsync(eventId, userOrg);

            Assert.That(result.ParticipatingStatus, Is.EqualTo(AttendingStatus.Idle));
            Assert.That(result.Participants, Is.Empty);
            Assert.That(result.MyChosenOptions, Is.Empty);
            Assert.That(result.Questions.SelectMany(q => q.Options).SelectMany(o => o.Participants), Is.Empty);
            Assert.That(result.GoingCount, Is.EqualTo(2));
        }

        [TestCase("[\"11\"]", new[] { "Eleven" }, false)]
        [TestCase("[\"1\",\"11\"]", new[] { "One", "Eleven" }, true)]
        [TestCase("[]", new string[0], false)]
        public async Task Should_Resolve_Office_Names_By_Id(string eventOffices, string[] expectedNames, bool expectedForAllOffices)
        {
            var eventId = MockEventDetailsWithLegacyAndQuestionOptions(eventOffices);
            _officeDbSet.SetDbSetDataForAsync(new List<Office>
            {
                new Office { Id = 1, Name = "One", OrganizationId = 2 },
                new Office { Id = 11, Name = "Eleven", OrganizationId = 2 }
            });

            var result = await _eventService.GetEventDetailsAsync(eventId, new UserAndOrganizationDto { OrganizationId = 2, UserId = "testUser1" });

            // "1" is a substring of ["11"], which the old lookup matched.
            Assert.That(result.Offices.OfficeNames, Is.EqualTo(expectedNames));
            Assert.That(result.IsForAllOffices, Is.EqualTo(expectedForAllOffices));
        }

        // The service reads participants, options and questions from their own tables, so each
        // mock is filled from the same object graph the event carries.
        private void SetEventDetailsData(Event @event)
        {
            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event }.AsQueryable());
            _eventParticipantsDbSet.SetDbSetDataForAsync(@event.EventParticipants.AsQueryable());
            _eventOptionsDbSet.SetDbSetDataForAsync(@event.EventOptions.AsQueryable());
            _eventQuestionsDbSet.SetDbSetDataForAsync(@event.EventQuestions.AsQueryable());
        }

        private Guid MockEventDetailsWithAnsweredQuestion(bool canSeeParticipants)
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), BasicPermissions.EventUsers, Arg.Any<CancellationToken>())
                .Returns(canSeeParticipants);

            var eventId = Guid.NewGuid();
            var responsibleUser = new ApplicationUser { Id = "responsibleUser1", FirstName = "user1f", LastName = "user1l" };

            var me = new EventParticipant
            {
                Id = 1,
                EventId = eventId,
                ApplicationUserId = "testUser1",
                ApplicationUser = new ApplicationUser { Id = "testUser1", FirstName = "Test", LastName = "User" },
                AttendStatus = (int)AttendingStatus.Attending,
                AttendComment = string.Empty
            };
            var colleague = new EventParticipant
            {
                Id = 2,
                EventId = eventId,
                ApplicationUserId = "otherUser",
                ApplicationUser = new ApplicationUser { Id = "otherUser", FirstName = "Other", LastName = "User" },
                AttendStatus = (int)AttendingStatus.Attending,
                AttendComment = string.Empty
            };
            var maybe = new EventParticipant
            {
                Id = 3,
                EventId = eventId,
                ApplicationUserId = "maybeUser",
                ApplicationUser = new ApplicationUser { Id = "maybeUser", FirstName = "Maybe", LastName = "User" },
                AttendStatus = (int)AttendingStatus.MaybeAttending,
                AttendComment = string.Empty
            };

            // All three picked Pasta (90); nobody picked Pizza (91).
            var pasta = new EventOption { Id = 90, EventId = eventId, Option = "Pasta", QuestionId = 5, Order = 0, EventParticipants = new List<EventParticipant> { me, colleague, maybe } };
            var pizza = new EventOption { Id = 91, EventId = eventId, Option = "Pizza", QuestionId = 5, Order = 1, EventParticipants = new List<EventParticipant>() };
            var margherita = new EventOption { Id = 92, EventId = eventId, Option = "Margherita", QuestionId = 6, Order = 0, EventParticipants = new List<EventParticipant>() };

            // A legacy option the colleague picked and the caller did not.
            var soup = new EventOption { Id = 80, EventId = eventId, Option = "Soup", QuestionId = null, Order = 0, EventParticipants = new List<EventParticipant> { colleague } };

            var @event = new Event
            {
                Id = eventId,
                OrganizationId = 2,
                Offices = "[\"1\"]",
                MaxChoices = 0,
                MaxParticipants = 2,
                ResponsibleUser = responsibleUser,
                ResponsibleUserId = responsibleUser.Id,
                EventOptions = new List<EventOption> { soup, pasta, pizza, margherita },
                EventParticipants = new List<EventParticipant> { me, colleague, maybe },
                EventQuestions = new List<EventQuestion>
                {
                    new EventQuestion
                    {
                        Id = 5, EventId = eventId, Title = "Pick your dish", Order = 0,
                        SelectType = EventQuestionSelectType.Single, IsRequired = true,
                        ShowIfOptionId = null, Options = new List<EventOption> { pasta, pizza }
                    },
                    new EventQuestion
                    {
                        Id = 6, EventId = eventId, Title = "Which pizza?", Order = 1,
                        SelectType = EventQuestionSelectType.Single, IsRequired = true,
                        ShowIfOptionId = 91, Options = new List<EventOption> { margherita }
                    }
                }
            };

            _officeDbSet.SetDbSetDataForAsync(new List<Office> { new Office { Id = 1, Name = "Office", OrganizationId = 2 } });
            SetEventDetailsData(@event);

            return eventId;
        }

        private Guid MockEventDetailsWithLegacyAndQuestionOptions(string offices = "[\"1\"]")
        {
            var eventId = Guid.NewGuid();
            var responsibleUser = new ApplicationUser
            {
                Id = "responsibleUser1",
                FirstName = "user1f",
                LastName = "user1l",
                PictureId = "host-picture.png"
            };

            var options = new List<EventOption>
            {
                new EventOption { Id = 80, EventId = eventId, Option = "Legacy option", QuestionId = null, Order = 0, EventParticipants = new List<EventParticipant>() },
                new EventOption { Id = 90, EventId = eventId, Option = "Pasta", QuestionId = 5, Order = 0, EventParticipants = new List<EventParticipant>() },
                new EventOption { Id = 91, EventId = eventId, Option = "Pizza", QuestionId = 5, Order = 1, EventParticipants = new List<EventParticipant>() }
            };

            var @event = new Event
            {
                Id = eventId,
                OrganizationId = 2,
                Offices = offices,
                MaxChoices = 1,
                ResponsibleUser = responsibleUser,
                ResponsibleUserId = responsibleUser.Id,
                EventOptions = options,
                // EF never hands back a null collection navigation; the mock must not either.
                EventQuestions = new List<EventQuestion>(),
                EventParticipants = new List<EventParticipant>()
            };

            _officeDbSet.SetDbSetDataForAsync(new List<Office> { new Office { Id = 1, Name = "Office", OrganizationId = 2 } });
            SetEventDetailsData(@event);

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
