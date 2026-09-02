using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.Services.Wall;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;
using Shrooms.Premium.Domain.Services.Email.Event;
using Shrooms.Premium.Domain.Services.Events.Participation;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    [TestFixture]
    public class EventParticipantServiceTests
    {
        private DbSet<Event> _eventsDbSet;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<EventOption> _eventOptionsDbSet;
        private DbSet<EventParticipant> _eventParticipantsDbSet;

        private IUnitOfWork2 _uow2;
        private ISystemClock _systemClockMock;
        private IEventParticipationService _eventParticipationService;
        private IEventValidationService _eventValidationServiceMock;
        private EventValidationService _eventValidationService;

        private IWallService _wallService;

        private IAsyncRunner _asyncRunner;

        private IEventAnswerValidator _eventAnswerValidator;

        [SetUp]
        public void TestInitializer()
        {
            _uow2 = Substitute.For<IUnitOfWork2>();

            _eventsDbSet = Substitute.For<DbSet<Event>, IQueryable<Event>, IAsyncEnumerable<Event>>();
            _uow2.GetDbSet<Event>().Returns(_eventsDbSet);

            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IAsyncEnumerable<ApplicationUser>>();
            _uow2.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            _eventParticipantsDbSet = Substitute.For<DbSet<EventParticipant>, IQueryable<EventParticipant>, IAsyncEnumerable<EventParticipant>>();
            _uow2.GetDbSet<EventParticipant>().Returns(_eventParticipantsDbSet);

            _eventOptionsDbSet = Substitute.For<DbSet<EventOption>, IQueryable<EventOption>, IAsyncEnumerable<EventOption>>();
            _uow2.GetDbSet<EventOption>().Returns(_eventOptionsDbSet);

            _systemClockMock = Substitute.For<ISystemClock>();
            var permissionService = Substitute.For<IPermissionService>();
            _eventValidationServiceMock = Substitute.For<IEventValidationService>();
            _eventValidationService = new EventValidationService(_systemClockMock);
            var roleService = Substitute.For<IRoleService>();
            MockRoleService(roleService);
            _wallService = Substitute.For<IWallService>();
            _asyncRunner = Substitute.For<IAsyncRunner>();
            _eventAnswerValidator = new EventAnswerValidator();

            _eventParticipationService =
                new EventParticipationService(
                    _uow2,
                    _systemClockMock,
                    roleService,
                    permissionService,
                    _eventValidationServiceMock,
                    _wallService,
                    _asyncRunner,
                    _eventAnswerValidator);
        }

        [Test]
        public async Task Should_Successfully_Join_Event_Without_Options()
        {
            var eventGuid = MockEventWithoutOptions();
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));
            var eventJoinDto = new EventJoinDto
            {
                ChosenOptions = new List<int>(),
                ParticipantIds = new List<string> { "testUserId" },
                EventId = eventGuid,
                UserId = "testUserId",
                OrganizationId = 2
            };

            await _eventParticipationService.JoinAsync(eventJoinDto);
            _eventParticipantsDbSet.Received(1).Add(Arg.Any<EventParticipant>());
        }

        [Test]
        public async Task Should_Successfully_Join_Event_Without_Options_When_Notifying_Managers()
        {
            // Arrange
            var eventGuid = MockEventWithoutOptions(true);

            MockUsers();

            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            var eventJoinDto = new EventJoinDto
            {
                ChosenOptions = new List<int>(),
                ParticipantIds = new List<string> { "testUserId" },
                EventId = eventGuid,
                UserId = "testUserId",
                OrganizationId = 2
            };

            // Act
            await _eventParticipationService.JoinAsync(eventJoinDto);

            // Assert
            _eventParticipantsDbSet.Received(1).Add(Arg.Any<EventParticipant>());
        }

        [Test]
        public async Task Should_Successfully_Join_Event_With_Options()
        {
            var eventGuid = MockEventWithOptions();
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));
            var eventJoinDto = new EventJoinDto
            {
                ChosenOptions = new List<int> { 1 },
                ParticipantIds = new List<string> { "testUserId" },
                EventId = eventGuid,
                UserId = "testUserId",
                OrganizationId = 2
            };

            await _eventParticipationService.JoinAsync(eventJoinDto);
            _eventParticipantsDbSet.Received(1).Add(Arg.Is<EventParticipant>(x => x.EventOptions.Count == 1));
        }

        [Test]
        public async Task Should_Successfully_Join_Event_With_Options_When_Notifying_Managers()
        {
            // Arrange
            var eventGuid = MockEventWithOptions(true);

            MockUsers();

            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            var eventJoinDto = new EventJoinDto
            {
                ChosenOptions = new List<int> { 1 },
                ParticipantIds = new List<string> { "testUserId" },
                EventId = eventGuid,
                UserId = "testUserId",
                OrganizationId = 2
            };

            // Act
            await _eventParticipationService.JoinAsync(eventJoinDto);

            // Assert
            _eventParticipantsDbSet.Received(1).Add(Arg.Is<EventParticipant>(x => x.EventOptions.Count == 1));
        }

        [Test]
        public void Should_Let_A_Question_Only_Event_Be_Joined_With_Answers()
        {
            var eventId = MockEventWithRequiredQuestion();

            // Use the real EventValidationService here (not the substitute) so the choice checks
            // actually run instead of being no-ops.
            _eventParticipationService = new EventParticipationService(
                _uow2,
                _systemClockMock,
                Substitute.For<IRoleService>(),
                Substitute.For<IPermissionService>(),
                _eventValidationService,
                _wallService,
                _asyncRunner,
                _eventAnswerValidator);

            var eventJoinDto = new EventJoinDto
            {
                // Answers q5 with Pasta. q6 is conditional on Pizza, so it stays hidden and
                // unanswered — a valid submission that today trips the MaxChoices check.
                ChosenOptions = new List<int> { 90 },
                EventId = eventId,
                ParticipantIds = new List<string> { "user1" },
                UserId = "user1",
                OrganizationId = 2,
                AttendStatus = AttendingStatus.Attending
            };

            Assert.DoesNotThrowAsync(async () => await _eventParticipationService.JoinAsync(eventJoinDto));
        }

        [Test]
        public void Should_Let_A_Mixed_Event_Be_Joined_With_A_Legacy_IgnoreSingleJoin_Option_And_A_Question_Answer()
        {
            var eventId = MockEventWithLegacyIgnoreSingleJoinOptionAndQuestion();

            // Use the real EventValidationService here (not the substitute) so the choice checks
            // actually run instead of being no-ops.
            _eventParticipationService = new EventParticipationService(
                _uow2,
                _systemClockMock,
                Substitute.For<IRoleService>(),
                Substitute.For<IPermissionService>(),
                _eventValidationService,
                _wallService,
                _asyncRunner,
                _eventAnswerValidator);

            var eventJoinDto = new EventJoinDto
            {
                // Option 10 is the legacy IgnoreSingleJoin option; 90 is the question answer.
                // Picking both must not trip CheckIfSingleChoiceSelectedWithRule.
                ChosenOptions = new List<int> { 10, 90 },
                EventId = eventId,
                ParticipantIds = new List<string> { "user1" },
                UserId = "user1",
                OrganizationId = 2,
                AttendStatus = AttendingStatus.Attending
            };

            Assert.DoesNotThrowAsync(async () => await _eventParticipationService.JoinAsync(eventJoinDto));
        }

        [Test]
        public void Should_Reject_A_Join_That_Skips_A_Required_Question()
        {
            var eventId = MockEventWithRequiredQuestion();
            var eventJoinDto = new EventJoinDto
            {
                ChosenOptions = new List<int>(),
                EventId = eventId,
                ParticipantIds = new List<string> { "user1" },
                UserId = "user1",
                OrganizationId = 2,
                AttendStatus = AttendingStatus.Attending
            };

            var ex = Assert.ThrowsAsync<EventAnswersInvalidException>(
                async () => await _eventParticipationService.JoinAsync(eventJoinDto));

            Assert.That(ex.Errors.Select(e => e.Reason),
                Is.EquivalentTo(new[] { EventAnswerErrorReason.RequiredAnswerMissing }));
        }

        [Test]
        public void Should_Reject_An_Answer_For_A_Hidden_Question()
        {
            var eventId = MockEventWithRequiredQuestion();
            // 92 belongs to the conditional question, whose trigger (91) was not chosen.
            var eventJoinDto = new EventJoinDto
            {
                ChosenOptions = new List<int> { 90, 92 },
                EventId = eventId,
                ParticipantIds = new List<string> { "user1" },
                UserId = "user1",
                OrganizationId = 2,
                AttendStatus = AttendingStatus.Attending
            };

            var ex = Assert.ThrowsAsync<EventAnswersInvalidException>(
                async () => await _eventParticipationService.JoinAsync(eventJoinDto));

            Assert.That(ex.Errors.Select(e => e.Reason),
                Is.EquivalentTo(new[] { EventAnswerErrorReason.AnswerForHiddenQuestion }));
        }

        [Test]
        public void Should_Accept_A_Join_That_Answers_Every_Reachable_Question()
        {
            var eventId = MockEventWithRequiredQuestion();
            var eventJoinDto = new EventJoinDto
            {
                ChosenOptions = new List<int> { 91, 92 },
                EventId = eventId,
                ParticipantIds = new List<string> { "user1" },
                UserId = "user1",
                OrganizationId = 2,
                AttendStatus = AttendingStatus.Attending
            };

            Assert.DoesNotThrowAsync(async () => await _eventParticipationService.JoinAsync(eventJoinDto));
        }

        [Test]
        public void Should_Validate_Answers_When_A_Host_Adds_Colleagues()
        {
            var eventId = MockEventWithRequiredQuestion();
            var eventJoinDto = new EventJoinDto
            {
                ChosenOptions = new List<int>(),
                EventId = eventId,
                ParticipantIds = new List<string> { "user1", "user2" },
                UserId = "host1",
                OrganizationId = 2,
                AttendStatus = AttendingStatus.Attending
            };

            Assert.ThrowsAsync<EventAnswersInvalidException>(
                async () => await _eventParticipationService.AddColleagueAsync(eventJoinDto));
        }

        [Test]
        public void Should_Reject_Edited_Choices_That_Skip_A_Required_Question()
        {
            var eventId = MockEventWithRequiredQuestion();

            var changeOptionsDto = new EventChangeOptionsDto
            {
                EventId = eventId,
                ChosenOptions = new List<int>(),

                // An empty Answers array is the caller saying "my answers are: none", which is what
                // puts the answer rules in play. Omitting it means "leave my answers alone".
                Answers = new List<int>(),
                UserId = "user1",
                OrganizationId = 2
            };

            Assert.ThrowsAsync<EventAnswersInvalidException>(
                async () => await _eventParticipationService.UpdateSelectedOptionsAsync(changeOptionsDto));
        }

        [Test]
        public void Should_Not_Enforce_Answers_When_A_Legacy_Payload_Carries_None()
        {
            // The shipped AngularJS client posts flat options only. Enforcing answers against what
            // is already stored would leave a participant unable to change options at all once a
            // host adds a required question to a live event.
            var eventId = MockEventWithRequiredQuestion(withAttendingUser1: true);

            var changeOptionsDto = new EventChangeOptionsDto
            {
                EventId = eventId,
                ChosenOptions = new List<int>(),
                UserId = "user1",
                OrganizationId = 2
            };

            Assert.DoesNotThrowAsync(
                async () => await _eventParticipationService.UpdateSelectedOptionsAsync(changeOptionsDto));
        }

        [Test]
        public void Should_Throw_If_Joining_Event_Is_Expired()
        {
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-06-21"));

            var registrationDeadline = DateTime.Parse("2016-06-20");
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfRegistrationDeadlineIsExpired(registrationDeadline));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventRegistrationDeadlineIsExpired));
        }

        [Test]
        public void Should_Throw_If_Joining_User_Provided_Not_Enough_Options()
        {
            const int maxChoices = 2;
            const int choicesProvided = 0;
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfJoiningNotEnoughChoicesProvided(maxChoices, choicesProvided));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventNotEnoughChoicesProvidedCode));
        }

        [Test]
        public void Should_Throw_If_Joining_User_Provided_Too_Many_Choices()
        {
            const int maxChoices = 2;
            const int choicesProvided = 3;
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfJoiningTooManyChoicesProvided(maxChoices, choicesProvided));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventTooManyChoicesProvidedCode));
        }

        [Test]
        public void Should_Throw_If_Joining_Event_Is_Already_Full()
        {
            const int maxParticipants = 5;
            const int participantsCount = 6;
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfEventHasEnoughPlaces(maxParticipants, participantsCount));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventIsFullCode));
        }

        [Test]
        public async Task Should_Return_If_Joining_Many_Users_Failed()
        {
            var eventGuid = MockEventWithOptions();
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-04-02"));
            var eventJoinDto = new EventJoinDto
            {
                ParticipantIds = new List<string> { "testUserId", "1" },
                ChosenOptions = new List<int> { 1 },
                EventId = eventGuid,
                OrganizationId = 2
            };

            await _eventParticipationService.JoinAsync(eventJoinDto);
            _eventParticipantsDbSet.Received(2).Add(Arg.Is<EventParticipant>(x => x.EventOptions.Count == 1));
            await _uow2.Received(1).SaveChangesAsync(false);
        }

        [Test]
        public async Task Should_Return_Participants_List()
        {
            var eventGuid = MockEventWithParticipants();
            MockUsers();
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            var result = (await _eventParticipationService.GetEventParticipantsAsync(eventGuid, userAndOrg)).ToList();

            var names = result.Select(participant => participant.FirstName).ToList();
            ClassicAssert.AreEqual(3, result.Count);
            CollectionAssert.DoesNotContain(names, "Gone");
            CollectionAssert.Contains(names, "Undecided");
            ClassicAssert.AreEqual("Name", result.First().FirstName);

            var choices = result.First().Choices.ToList();
            ClassicAssert.AreEqual(2, choices.Count);
            ClassicAssert.AreEqual("Vegan", choices[0].Option);
            ClassicAssert.AreEqual(3, choices[0].QuestionOrder);
            ClassicAssert.AreEqual("Pizza", choices[1].Option);
            ClassicAssert.AreEqual(null, choices[1].QuestionOrder);
            ClassicAssert.IsEmpty(result.Last().Choices);
        }

        [Test]
        public void Should_Throw_When_Paricipant_Was_Not_Found()
        {
            MockLeaveEvent();
            var eventId = new Guid();
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "user"
            };

            Assert.ThrowsAsync<EventException>(async () => await _eventParticipationService.LeaveAsync(eventId, userOrg, "leave comment"));
        }

        [Test]
        public async Task Should_Leave_Event_That_Has_Not_Started()
        {
            var eventId = MockLeaveEvent();
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "user"
            };

            await _eventParticipationService.LeaveAsync(eventId, userOrg, "leave comment");
            _eventParticipantsDbSet.Received(1).Remove(Arg.Any<EventParticipant>());
        }

        [Test]
        public async Task Should_Throw_When_Participant_Wasnt_removed()
        {
            var eventId = MockRemoveParticipant();
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "user"
            };

            await _eventParticipationService.ExpelAsync(eventId, userOrg, "user2");
            _eventParticipantsDbSet.Received(1).Remove(Arg.Any<EventParticipant>());
        }

        [Test]
        public async Task Should_Throw_When_User_Has_No_Permission_To_Remove_Participant()
        {
            var eventId = MockRemoveParticipant();
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "user"
            };

            await _eventParticipationService.ExpelAsync(eventId, userOrg, "user2");
            _eventValidationServiceMock.Received(1).CheckIfUserHasPermission("user", "user", false);
        }

        [Test]
        public async Task Should_Throw_If_Trying_To_Expell_Participant_From_Finished_Event()
        {
            var eventId = MockRemoveParticipantWithExpiredEvent();
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "user"
            };

            await _eventParticipationService.ExpelAsync(eventId, userOrg, "user2");
            _eventValidationServiceMock.Received(1).CheckIfEventEndDateIsExpired(Arg.Is<DateTime>(x => x < DateTime.UtcNow));
        }

        [Test]
        public void Should_Throw_If_Participant_Does_Not_Exist()
        {
            EventParticipant noParticipant = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<EventException>(() => _eventValidationService.CheckIfParticipantExists(noParticipant));
        }

        [Test]
        public void Should_Not_Throw_If_Participant_Exists()
        {
            var participant = new EventParticipant();
            Assert.DoesNotThrow(() => _eventValidationService.CheckIfParticipantExists(participant));
        }

        [Test]
        public async Task Should_Reset_All_Event_Participants()
        {
            var @event = MockResetAttendees();
            var user = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "user1"
            };

            await _eventParticipationService.ResetAllAttendeesAsync(@event.Id, user);

            _eventParticipantsDbSet.Received(1).RemoveRange(Arg.Any<IEnumerable<EventParticipant>>());
        }

        [Test]
        public async Task Should_Reset_Event_Participants()
        {
            var @event = MockResetAttendees();
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "user1"
            };

            await _eventParticipationService.ResetAttendeesAsync(@event, userOrg);

            _eventParticipantsDbSet.Received(1).RemoveRange(Arg.Any<IEnumerable<EventParticipant>>());
        }

        [Test]
        public async Task Should_Reset_Virtual_Event_Participants()
        {
            var @event = MockResetAttendees();
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "user1"
            };

            await _eventParticipationService.ResetVirtualAttendeesAsync(@event, userOrg);

            _eventParticipantsDbSet.Received(1).RemoveRange(Arg.Any<IEnumerable<EventParticipant>>());
        }

        [Test]
        public void Should_Throw_If_User_Is_Not_Creator_Or_Event_Admin()
        {
            const string userId = "user";
            const string eventCreatorId = "creator";
            const bool isAdmin = false;
            Assert.Throws<EventException>(() => _eventValidationService.CheckIfUserHasPermission(userId, eventCreatorId, isAdmin));
        }

        [TestCase(AttendingStatus.MaybeAttending)]
        [TestCase(AttendingStatus.Idle)]
        [TestCase(AttendingStatus.NotAttending)]
        [TestCase((AttendingStatus)int.MaxValue)]
        public void Should_Throw_If_Invalid_Attend_Status_Given_For_Join(AttendingStatus status)
        {
            Assert.Throws<EventException>(() => _eventValidationService.CheckIfJoinAttendStatusIsValid(status, new EventJoinValidationDto()));
        }

        [TestCase(AttendingStatus.Attending)]
        [TestCase(AttendingStatus.AttendingVirtually)]
        public void Should_Not_Throw_If_Valid_Attend_Status_Given_For_Join(AttendingStatus status)
        {
            var validationDto = new EventJoinValidationDto
            {
                MaxParticipants = 1,
                MaxVirtualParticipants = 1
            };

            Assert.DoesNotThrow(() => _eventValidationService.CheckIfJoinAttendStatusIsValid(status, validationDto));
        }

        [Test]
        public void Should_Throw_If_Can_Not_Join_Virtually()
        {
            var status = AttendingStatus.AttendingVirtually;
            var validationDto = new EventJoinValidationDto
            {
                MaxParticipants = 1,
                MaxVirtualParticipants = 0
            };

            Assert.Throws<EventException>(() => _eventValidationService.CheckIfJoinAttendStatusIsValid(status, validationDto));
        }

        [Test]
        public void Should_Not_Throw_If_Can_Join_Virtually()
        {
            var status = AttendingStatus.AttendingVirtually;
            var validationDto = new EventJoinValidationDto
            {
                MaxParticipants = 1,
                MaxVirtualParticipants = 1
            };

            Assert.DoesNotThrow(() => _eventValidationService.CheckIfJoinAttendStatusIsValid(status, validationDto));
        }

        [Test]
        public void Should_Throw_If_Can_Not_Join()
        {
            var status = AttendingStatus.Attending;
            var validationDto = new EventJoinValidationDto
            {
                MaxParticipants = 0,
                MaxVirtualParticipants = 0
            };

            Assert.Throws<EventException>(() => _eventValidationService.CheckIfJoinAttendStatusIsValid(status, validationDto));
        }

        [Test]
        public void Should_Not_Throw_If_Can_Join()
        {
            var status = AttendingStatus.Attending;
            var validationDto = new EventJoinValidationDto
            {
                MaxParticipants = 1,
                MaxVirtualParticipants = 0
            };

            Assert.DoesNotThrow(() => _eventValidationService.CheckIfJoinAttendStatusIsValid(status, validationDto));
        }

        [Test]
        public void Should_Throw_If_Not_All_Participants_Exist()
        {
            var foundUsers = new List<ApplicationUser>();
            var participantIds = new List<string> { "id" };
            Assert.Throws<EventException>(() => _eventValidationService.CheckIfAllParticipantsExist(foundUsers, participantIds));
        }

        [Test]
        public void Should_Not_Throw_If_All_Participants_Exist()
        {
            var foundUsers = new List<ApplicationUser> { new ApplicationUser() };
            var participantIds = new List<string> { "id" };
            Assert.DoesNotThrow(() => _eventValidationService.CheckIfAllParticipantsExist(foundUsers, participantIds));
        }

        [TestCase(AttendingStatus.Attending)]
        [TestCase(AttendingStatus.AttendingVirtually)]
        public void Should_Not_Throw_If_User_Can_Join_Event(AttendingStatus status)
        {
            var joinDto = new EventJoinDto
            {
                AttendStatus = status,
                ParticipantIds = new List<string> { "1", "2", "3" }
            };

            var validationDto = new EventJoinValidationDto
            {
                MaxVirtualParticipants = 3,
                MaxParticipants = 3,
                Participants = new List<EventParticipantAttendDto>()
            };

            Assert.DoesNotThrow(() => _eventValidationService.CheckIfCanJoinEvent(joinDto, validationDto));
        }

        [TestCase(AttendingStatus.Attending)]
        [TestCase(AttendingStatus.AttendingVirtually)]
        public void Should_Throw_If_Event_Participants_Capacity_Reached(AttendingStatus status)
        {
            var joinDto = new EventJoinDto
            {
                AttendStatus = status,
                ParticipantIds = new List<string> { "1", "2", "3" }
            };

            var validationDto = new EventJoinValidationDto
            {
                MaxVirtualParticipants = 2,
                MaxParticipants = 2,
                Participants = new List<EventParticipantAttendDto>()
            };

            Assert.Throws<EventException>(() => _eventValidationService.CheckIfCanJoinEvent(joinDto, validationDto));
        }

        [Test]
        public void Should_Not_Throw_If_User_Is_Creator_And_Not_Event_Admin()
        {
            const string userId = "creator";
            const string eventCreatorId = "creator";
            const bool isAdmin = false;
            Assert.DoesNotThrow(() => _eventValidationService.CheckIfUserHasPermission(userId, eventCreatorId, isAdmin));
        }

        [Test]
        public void Should_Not_Throw_If_User_Is_Event_Administrator()
        {
            const string userId = "user";
            const string eventCreatorId = "creator";
            const bool isAdmin = true;
            Assert.DoesNotThrow(() => _eventValidationService.CheckIfUserHasPermission(userId, eventCreatorId, isAdmin));
        }

        [Test]
        public async Task Should_Return_Only_Not_Participating_Users_While_Searching_For_Users_To_Join_Event()
        {
            var eventId = MockEventAndUsersForSearch();
            const string searchString = "user";
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            var result = (await _eventParticipationService.SearchForEventJoinAutocompleteAsync(eventId, searchString, userOrg)).ToList();
            ClassicAssert.AreEqual(3, result.Count);
            ClassicAssert.IsFalse(result.Any(x => x.Id == "user1"));
            ClassicAssert.IsFalse(result.Any(x => x.Id == "user2"));
        }

        [Test]
        public async Task Should_Search_By_UserName()
        {
            var eventId = MockEventAndUsersForSearch2();
            const string searchString = "clyde";
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            var result = (await _eventParticipationService.SearchForEventJoinAutocompleteAsync(eventId, searchString, userOrg)).ToList();
            ClassicAssert.AreEqual(1, result.Count);
            ClassicAssert.AreEqual("user4", result.First().Id);
        }

        [Test]
        public async Task Should_Search_By_FirstName()
        {
            var eventId = MockEventAndUsersForSearch2();
            const string searchString = "User1f";
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };
            var result = (await _eventParticipationService.SearchForEventJoinAutocompleteAsync(eventId, searchString, userOrg)).ToList();
            ClassicAssert.AreEqual(1, result.Count);
            ClassicAssert.AreEqual("user1", result.First().Id);
        }

        [Test]
        public async Task Should_Search_By_LastName()
        {
            var eventId = MockEventAndUsersForSearch2();
            const string searchString = "User1l";
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            var result = (await _eventParticipationService.SearchForEventJoinAutocompleteAsync(eventId, searchString, userOrg)).ToList();
            ClassicAssert.AreEqual(1, result.Count);
            ClassicAssert.AreEqual("user1", result.First().Id);
        }

        [Test]
        public async Task Should_Search_By_FullName()
        {
            var eventId = MockEventAndUsersForSearch2();
            const string searchString = "User1fname user1l";
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            var result = (await _eventParticipationService.SearchForEventJoinAutocompleteAsync(eventId, searchString, userOrg)).ToList();
            ClassicAssert.AreEqual(1, result.Count);
            ClassicAssert.AreEqual("user1", result.First().Id);
        }

        [Test]
        public void Should_Throw_When_Event_Not_Found()
        {
            var dto = new EventChangeOptionsDto { EventId = Guid.Empty, OrganizationId = 1 };
            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { new Event { Id = Guid.NewGuid(), OrganizationId = 1 } }.AsQueryable());
            _eventValidationServiceMock
                .When(x => x.CheckIfEventExists((object)null))
                .Do(_ => throw new EventException(PremiumErrorCodes.EventDoesNotExistCode));

            Assert.ThrowsAsync<EventException>(async () => await _eventParticipationService.UpdateSelectedOptionsAsync(dto), PremiumErrorCodes.EventDoesNotExistCode);
        }

        [Test]
        public void Should_Throw_When_Registration_Deadline_Expired()
        {
            var guid = MockEventWithOptions();
            var dto = new EventChangeOptionsDto { EventId = guid, OrganizationId = 2, ChosenOptions = new List<int> { 1 } };
            _eventValidationServiceMock
                .When(x => x.CheckIfRegistrationDeadlineIsExpired(DateTime.Parse("2016-04-05")))
                .Do(_ => throw new EventException(PremiumErrorCodes.EventRegistrationDeadlineIsExpired));

            Assert.ThrowsAsync<EventException>(async () => await _eventParticipationService.UpdateSelectedOptionsAsync(dto), PremiumErrorCodes.EventRegistrationDeadlineIsExpired);
        }

        [Test]
        public void Should_Throw_When_Chosen_Options_Invalid()
        {
            var guid = MockEventWithOptions();
            var chosenOptionIds = new List<int> { -9999 };
            var dto = new EventChangeOptionsDto { EventId = guid, OrganizationId = 2, ChosenOptions = chosenOptionIds };

            _eventValidationServiceMock
                .When(x => x.CheckIfProvidedOptionsAreValid(Arg.Is<IEnumerable<int>>(a => a.SequenceEqual(chosenOptionIds)), Arg.Is<ICollection<EventOption>>(a => a.Count == 0)))
                .Do(_ => throw new EventException(PremiumErrorCodes.EventRegistrationDeadlineIsExpired));

            // CheckIfProvidedOptionsAreValid still owns the unknown-option rejection, ahead of the
            // answer validator: it answers with code 215, which the web client has a message for.
            Assert.ThrowsAsync<EventException>(async () => await _eventParticipationService.UpdateSelectedOptionsAsync(dto), PremiumErrorCodes.EventNoSuchOptionsCode);
        }

        [Test]
        public void Should_Throw_When_Not_Enough_Options_Chosen()
        {
            var guid = MockEventWithOptions();
            var chosenOptionIds = new List<int>();
            var dto = new EventChangeOptionsDto { EventId = guid, OrganizationId = 2, ChosenOptions = chosenOptionIds };

            _eventValidationServiceMock
                .When(x => x.CheckIfJoiningNotEnoughChoicesProvided(1, 0))
                .Do(_ => throw new EventException(PremiumErrorCodes.EventNotEnoughChoicesProvidedCode));

            Assert.ThrowsAsync<EventException>(async () => await _eventParticipationService.UpdateSelectedOptionsAsync(dto), PremiumErrorCodes.EventNotEnoughChoicesProvidedCode);
        }

        [Test]
        public void Should_Throw_When_Too_Many_Options_Chosen()
        {
            var guid = MockEventWithOptions();
            var chosenOptionIds = new List<int> { 1, 2, 3 };
            var dto = new EventChangeOptionsDto { EventId = guid, OrganizationId = 2, ChosenOptions = chosenOptionIds };

            _eventValidationServiceMock
                .When(x => x.CheckIfJoiningTooManyChoicesProvided(1, 3))
                .Do(_ => throw new EventException(PremiumErrorCodes.EventTooManyChoicesProvidedCode));

            Assert.ThrowsAsync<EventException>(async () => await _eventParticipationService.UpdateSelectedOptionsAsync(dto), PremiumErrorCodes.EventTooManyChoicesProvidedCode);
        }

        [Test]
        public void Should_Throw_When_Single_Join_Rule_Chosen_With_More_Options()
        {
            var guid = MockEventWithOptions();
            var chosenOptionIds = new List<int> { 1, 4 };
            var dto = new EventChangeOptionsDto { EventId = guid, OrganizationId = 2, ChosenOptions = chosenOptionIds };

            _eventValidationServiceMock
                .When(x => x.CheckIfSingleChoiceSelectedWithRule(Arg.Is<ICollection<EventOption>>(a =>
                    a.Any(e => e.Rule == OptionRules.IgnoreSingleJoin) && a.Count > 1), OptionRules.IgnoreSingleJoin))
                .Do(_ => throw new EventException(PremiumErrorCodes.EventChoiceCanBeSingleOnly));

            Assert.ThrowsAsync<EventException>(async () => await _eventParticipationService.UpdateSelectedOptionsAsync(dto), PremiumErrorCodes.EventChoiceCanBeSingleOnly);
        }

        [Test]
        public void Should_Throw_When_User_Is_Not_Participating_In_Event()
        {
            var guid = MockEventWithOptions();
            var chosenOptionIds = new List<int> { 1 };
            var dto = new EventChangeOptionsDto { EventId = guid, OrganizationId = 2, ChosenOptions = chosenOptionIds, UserId = "1foo2bar" };

            _eventValidationServiceMock
                .When(x => x.CheckIfUserParticipatesInEvent("1foo2bar", Arg.Any<IEnumerable<EventParticipantAttendDto>>()))
                .Do(_ => throw new EventException(PremiumErrorCodes.EventUserNotParticipating));

            Assert.ThrowsAsync<EventException>(async () => await _eventParticipationService.UpdateSelectedOptionsAsync(dto), PremiumErrorCodes.EventUserNotParticipating);
        }

        [Test]
        public async Task Should_Send_Email_To_Manager_When_User_Successfully_Joined_Event_Without_Options()
        {
            // Arrange
            var eventGuid = MockEventWithoutOptions(true);
            MockUsers();

            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            var eventJoinDto = new EventJoinDto
            {
                ChosenOptions = new List<int>(),
                ParticipantIds = new List<string> { "testUserId", "1" },
                EventId = eventGuid,
                UserId = "testUserId",
                OrganizationId = 2
            };

            // Act
            await _eventParticipationService.JoinAsync(eventJoinDto);

            // Assert
            _asyncRunner.Received(2).Run(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Send_Email_To_Manager_When_User_Successfully_Joined_Event_With_Options()
        {
            // Arrange
            var eventGuid = MockEventWithOptions(true);

            MockUsers();

            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            var eventJoinDto = new EventJoinDto
            {
                ChosenOptions = new List<int> { 1 },
                ParticipantIds = new List<string> { "testUserId", "1" },
                EventId = eventGuid,
                UserId = "testUserId",
                OrganizationId = 2
            };

            // Act
            await _eventParticipationService.JoinAsync(eventJoinDto);

            // Assert
            _asyncRunner.Received(2).Run(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Not_Send_Email_To_Manager_When_User_Joins_With_Options_And_Event_Type_Does_Not_Require_It()
        {
            // Arrange
            var eventGuid = MockEventWithOptions(false);

            MockUsers();

            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            var eventJoinDto = new EventJoinDto
            {
                ChosenOptions = new List<int> { 1 },
                ParticipantIds = new List<string> { "testUserId", "1" },
                EventId = eventGuid,
                UserId = "testUserId",
                OrganizationId = 2
            };

            // Act
            await _eventParticipationService.JoinAsync(eventJoinDto);

            // Assert
            _asyncRunner.Received(0).Run(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Not_Send_Email_To_Manager_When_User_Joins_Without_Options_And_Event_Type_Does_Not_Require_It()
        {
            // Arrange
            var eventGuid = MockEventWithOptions(false);

            MockUsers();

            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            var eventJoinDto = new EventJoinDto
            {
                ChosenOptions = new List<int> { 1 },
                ParticipantIds = new List<string> { "testUserId", "1" },
                EventId = eventGuid,
                UserId = "testUserId",
                OrganizationId = 2
            };

            // Act
            await _eventParticipationService.JoinAsync(eventJoinDto);

            // Assert
            _asyncRunner.Received(0).Run(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Send_Email_To_Manager_When_Multiple_Users_Are_Added_And_Successfully_Joined_Event()
        {
            // Arrange
            var eventGuid = MockEventWithOptions(true);

            MockUsers();

            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            var eventJoinDto = new EventJoinDto
            {
                ChosenOptions = new List<int> { 1 },
                ParticipantIds = new List<string> { "testUserId", "1" },
                EventId = eventGuid,
                UserId = "testUserId",
                OrganizationId = 2
            };

            // Act
            await _eventParticipationService.AddColleagueAsync(eventJoinDto);

            // Assert
            _asyncRunner.Received(2).Run(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Not_Send_Email_To_Manager_When_Multiple_Users_Are_Added_And_Event_Type_Does_Not_Require_It()
        {
            // Arrange
            var eventGuid = MockEventWithOptions(false);

            MockUsers();

            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            var eventJoinDto = new EventJoinDto
            {
                ChosenOptions = new List<int> { 1 },
                ParticipantIds = new List<string> { "testUserId", "1" },
                EventId = eventGuid,
                UserId = "testUserId",
                OrganizationId = 2
            };

            // Act
            await _eventParticipationService.AddColleagueAsync(eventJoinDto);

            // Assert
            _asyncRunner.Received(0).Run(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Send_Email_To_Manager_When_Event_Participants_Are_Resetted()
        {
            // Arrange
            var @event = MockResetAttendees(true);
            var extraCallForNotifyingUsersAboutBeingRemoved = 1;
            var user = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "user1"
            };

            // Act
            await _eventParticipationService.ResetAllAttendeesAsync(@event.Id, user);


            // Assert
            _asyncRunner.Received(3 + extraCallForNotifyingUsersAboutBeingRemoved)
                .Run(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Not_Send_Email_To_Manager_When_Event_Participants_Are_Resetted_And_Event_Type_Does_Not_Require_It()
        {
            // Arrange
            var @event = MockResetAttendees(false);
            var extraCallForNotifyingUsersAboutBeingRemoved = 1;

            MockUsers();

            var user = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "user1"
            };

            // Act
            await _eventParticipationService.ResetAllAttendeesAsync(@event.Id, user);


            // Assert
            _asyncRunner.Received(extraCallForNotifyingUsersAboutBeingRemoved)
                .Run(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Send_Email_To_Manager_When_Event_Participant_Is_Expelled()
        {
            // Arrange
            var eventId = MockRemoveParticipant(true);
            var extraCallForNotifyingUsersAboutBeingRemoved = 1;

            MockUsers();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "user"
            };

            // Act
            await _eventParticipationService.ExpelAsync(eventId, userOrg, "user2");

            // Assert
            _asyncRunner.Received(1 + extraCallForNotifyingUsersAboutBeingRemoved)
                .Run(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Not_Send_Email_To_Manager_When_Event_Participant_Is_Expelled_And_Event_Type_Does_Not_Require_It()
        {
            // Arrange
            var eventId = MockRemoveParticipant(false);
            var extraCallForNotifyingUsersAboutBeingRemoved = 1;

            MockUsers();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "user"
            };

            // Act
            await _eventParticipationService.ExpelAsync(eventId, userOrg, "user2");

            // Assert
            _asyncRunner.Received(extraCallForNotifyingUsersAboutBeingRemoved)
                .Run(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Expel_Event_Participant_When_Notifying_Manager()
        {
            // Arrange
            var eventId = MockRemoveParticipant(true);

            MockUsers();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "user"
            };

            // Act
            await _eventParticipationService.ExpelAsync(eventId, userOrg, "user2");

            // Assert
            _eventParticipantsDbSet.Received(1).Remove(Arg.Any<EventParticipant>());
        }

        #region Mocks

        private static void MockRoleService(IRoleService roleService)
        {
            var newRoleId = Guid.NewGuid().ToString();
            roleService.GetRoleIdByNameAsync(Roles.NewUser).Returns(newRoleId);

            roleService.ExcludeUsersWithRole(newRoleId).ReturnsForAnyArgs(x => true);
        }

        private Guid MockEventWithParticipants()
        {
            var guid = new Guid();
            var @event = new List<Event>
            {
                new Event
                {
                    Id = guid,
                    OrganizationId = 2,
                    Name = "Test event",
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant
                        {
                            ApplicationUser = new ApplicationUser
                            {
                                FirstName = "Name",
                                LastName = "Surname"
                            },
                            AttendStatus = 1,
                            EventOptions = new List<EventOption>
                            {
                                new EventOption
                                {
                                    Option = "Vegan",
                                    Order = 0,
                                    QuestionId = 7,
                                    Question = new EventQuestion { Id = 7, Title = "Dietary needs", Order = 3 }
                                },
                                new EventOption
                                {
                                    Option = "Pizza",
                                    Order = 1
                                }
                            }
                        },
                        new EventParticipant
                        {
                            ApplicationUser = new ApplicationUser
                            {
                                FirstName = "Name1",
                                LastName = "Surname1"
                            },
                            AttendStatus = 1,
                            EventOptions = new List<EventOption>()
                        },

                        new EventParticipant
                        {
                            ApplicationUser = new ApplicationUser
                            {
                                FirstName = "Undecided",
                                LastName = "Person"
                            },
                            AttendStatus = (int)AttendingStatus.MaybeAttending,
                            EventOptions = new List<EventOption>()
                        },

                        // Dropped out but kept the pick, which is what declining does.
                        new EventParticipant
                        {
                            ApplicationUser = new ApplicationUser
                            {
                                FirstName = "Gone",
                                LastName = "Away"
                            },
                            AttendStatus = (int)AttendingStatus.NotAttending,
                            EventOptions = new List<EventOption>
                            {
                                new EventOption { Option = "Vegan", Order = 0, QuestionId = 7 }
                            }
                        }
                    }
                }
            };

            _eventsDbSet.SetDbSetDataForAsync(@event.AsQueryable());
            return guid;
        }

        private Guid MockEventAndUsersForSearch2()
        {
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Id = eventId,
                EventParticipants = new List<EventParticipant>(),
                OrganizationId = 2
            };

            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "user1",
                    FirstName = "user1fname",
                    LastName = "user1lname",
                    OrganizationId = 2,
                    UserName = "john"
                },
                new ApplicationUser
                {
                    Id = "user2",
                    FirstName = "user2fname",
                    LastName = "user2lname",
                    OrganizationId = 2,
                    UserName = "Peter"
                },
                new ApplicationUser
                {
                    Id = "user3",
                    FirstName = "user3fname",
                    LastName = "user3lname",
                    OrganizationId = 2,
                    UserName = "Andrew"
                },
                new ApplicationUser
                {
                    Id = "user4",
                    FirstName = "user4fname",
                    LastName = "user4lname",
                    OrganizationId = 2,
                    UserName = "Clyde"
                },
                new ApplicationUser
                {
                    Id = "user5",
                    FirstName = "user5fname",
                    LastName = "user5lname",
                    OrganizationId = 2,
                    UserName = "Sam"
                }
            };

            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event }.AsQueryable());
            _usersDbSet.SetDbSetDataForAsync(users.AsQueryable());
            return eventId;
        }

        private Guid MockEventAndUsersForSearch()
        {
            var eventId = Guid.NewGuid();
            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    ApplicationUserId = "user1"
                },
                new EventParticipant
                {
                    ApplicationUserId = "user2"
                }
            };
            var @event = new Event
            {
                Id = eventId,
                EventParticipants = participants,
                OrganizationId = 2
            };

            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "user1",
                    FirstName = "user1",
                    LastName = "user1",
                    OrganizationId = 2,
                    UserName = "john"
                },
                new ApplicationUser
                {
                    Id = "user2",
                    FirstName = "user2",
                    LastName = "user2",
                    OrganizationId = 2,
                    UserName = "Peter"
                },
                new ApplicationUser
                {
                    Id = "user3",
                    FirstName = "user3",
                    LastName = "user3",
                    OrganizationId = 2,
                    UserName = "Andrew"
                },
                new ApplicationUser
                {
                    Id = "user4",
                    FirstName = "user4",
                    LastName = "user4",
                    OrganizationId = 2,
                    UserName = "Clyde"
                },
                new ApplicationUser
                {
                    Id = "user5",
                    FirstName = "user5",
                    LastName = "user5",
                    OrganizationId = 2,
                    UserName = "Sam"
                }
            };

            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event }.AsQueryable());
            _usersDbSet.SetDbSetDataForAsync(users.AsQueryable());
            return eventId;
        }

        private Event MockResetAttendees(bool sendEmailToManager = false)
        {
            var eventId = Guid.NewGuid();

            var @event = new Event
            {
                Name = "test",
                Id = eventId,
                RegistrationDeadline = DateTime.Parse("2016-06-20"),
                OrganizationId = 2,
                EventType = new EventType
                {
                    SendEmailToManager = sendEmailToManager
                }
            };

            var option1 = new EventOption
            {
                Id = 1,
                EventId = eventId,
                Event = @event,
                Option = "Option1"
            };
            var option2 = new EventOption
            {
                Id = 2,
                EventId = eventId,
                Event = @event,
                Option = "Option2"
            };
            var option3 = new EventOption
            {
                Id = 3,
                EventId = eventId,
                Event = @event,
                Option = "Option3"
            };

            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    Event = @event,
                    EventOptions = new List<EventOption> { option1 },
                    EventId = eventId,
                    ApplicationUserId = "user1",
                    ApplicationUser = new ApplicationUser
                    {
                        Id = "user1",
                        ManagerId = "testUserId",
                        Manager = new ApplicationUser
                        {
                            Email = "email",
                            Id = "testUserId"
                        }
                    }
                },
                new EventParticipant
                {
                    Id = 2,
                    Event = @event,
                    EventOptions = new List<EventOption> { option2 },
                    EventId = eventId,
                    ApplicationUserId = "user2",
                    ApplicationUser = new ApplicationUser
                    {
                        Id = "user2",
                        ManagerId = "testUserId",
                        Manager = new ApplicationUser
                        {
                            Email = "email",
                            Id = "testUserId"
                        }
                    }
                },
                new EventParticipant
                {
                    Id = 3,
                    Event = @event,
                    EventOptions = new List<EventOption> { option3 },
                    EventId = eventId,
                    ApplicationUserId = "user3",
                    ApplicationUser = new ApplicationUser
                    {
                        Id = "user3",
                        ManagerId = "testUserId",
                        Manager = new ApplicationUser
                        {
                            Email = "email",
                            Id = "testUserId"
                        }
                    }
                }
            };
            @event.EventOptions = new List<EventOption> { option1, option2, option3 };
            @event.EventParticipants = participants;

            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event }.AsQueryable());
            _eventParticipantsDbSet.SetDbSetDataForAsync(participants);
            return @event;
        }

        private Guid MockLeaveEvent()
        {
            _eventValidationServiceMock.When(x => x.CheckIfParticipantExists(null)).Do(_ => throw new EventException("Exception"));
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Name = "test",
                Id = eventId,
                RegistrationDeadline = DateTime.Parse("2016-06-20"),
                OrganizationId = 2,
                EventType = new EventType
                {
                    SendEmailToManager = false
                }
            };

            var eventOptions = new List<EventOption>
            {
                new EventOption
                {
                    Id = 1,
                    EventId = eventId,
                    Event = @event,
                    Option = "Option1"
                },
                new EventOption
                {
                    Id = 2,
                    EventId = eventId,
                    Event = @event,
                    Option = "Option2"
                }
            };

            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    Event = @event,
                    EventOptions = eventOptions,
                    EventId = eventId,
                    ApplicationUserId = "user",
                    AttendStatus = 1
                }
            };

            _eventParticipantsDbSet.SetDbSetDataForAsync(participants.AsQueryable());
            return eventId;
        }

        private Guid MockRemoveParticipantWithExpiredEvent()
        {
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Name = "test",
                Id = eventId,
                RegistrationDeadline = DateTime.Parse("2016-06-20"),
                EndDate = DateTime.Parse("2015-01-01"),
                OrganizationId = 2,
                ResponsibleUserId = "user",
                EventType = new EventType
                {
                    SendEmailToManager = false
                }
            };

            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    Event = @event,
                    EventOptions = new List<EventOption>(),
                    EventId = eventId,
                    ApplicationUserId = "user",
                    AttendStatus = 1
                },
                new EventParticipant
                {
                    Id = 2,
                    Event = @event,
                    EventOptions = new List<EventOption>(),
                    EventId = eventId,
                    ApplicationUserId = "user2",
                    AttendStatus = 1
                }
            };

            _eventParticipantsDbSet.SetDbSetDataForAsync(participants.AsQueryable());
            return eventId;
        }

        private Guid MockRemoveParticipant(bool sendEmailToManager = false)
        {
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Name = "test",
                Id = eventId,
                RegistrationDeadline = DateTime.Parse("2016-06-20"),
                OrganizationId = 2,
                ResponsibleUserId = "user",
                EventType = new EventType
                {
                    SendEmailToManager = sendEmailToManager
                }
            };

            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    Event = @event,
                    EventOptions = new List<EventOption>(),
                    EventId = eventId,
                    ApplicationUserId = "user",
                    AttendStatus = 1,
                    ApplicationUser = new ApplicationUser
                    {
                        ManagerId = "testUserId",
                        Email = "testUserId"
                    }
                },
                new EventParticipant
                {
                    Id = 2,
                    Event = @event,
                    EventOptions = new List<EventOption>(),
                    EventId = eventId,
                    ApplicationUserId = "user2",
                    AttendStatus = 1,
                    ApplicationUser = new ApplicationUser
                    {
                        ManagerId = "testUserId",
                        Email = "testUserId"
                    }
                }
            };

            _eventParticipantsDbSet.SetDbSetDataForAsync(participants.AsQueryable());
            return eventId;
        }

        private Guid MockEventWithoutOptions(bool sendEmailToManager = false)
        {
            var guid = Guid.NewGuid();
            var @event = new List<Event>
            {
                new Event
                {
                    StartDate = DateTime.Parse("2016-04-05"),
                    EndDate = DateTime.Parse("2016-04-06"),
                    RegistrationDeadline = DateTime.Parse("2016-04-05"),
                    EventOptions = new List<EventOption>(),
                    EventQuestions = new List<EventQuestion>(),
                    Id = guid,
                    MaxChoices = 0,
                    MaxParticipants = 20,
                    OrganizationId = 2,
                    Name = "Test event",
                    EventType = new EventType
                    {
                        Name = "test type",
                        IsSingleJoin = false,
                        Id = 1,
                        SendEmailToManager = sendEmailToManager
                    },
                    EventParticipants = new List<EventParticipant>(),
                    EventTypeId = 1
                }
            };

            var defaultEvent = @event.First();

            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    Event = defaultEvent,
                    EventOptions = new List<EventOption>(),
                    EventId = defaultEvent.Id,
                    ApplicationUserId = "user",
                    AttendStatus = 1
                },
                new EventParticipant
                {
                    Id = 2,
                    Event = defaultEvent,
                    EventOptions = new List<EventOption>(),
                    EventId = defaultEvent.Id,
                    ApplicationUserId = "user2",
                    AttendStatus = 1
                }
            };

            _eventParticipantsDbSet.SetDbSetDataForAsync(participants.AsQueryable());
            _eventsDbSet.SetDbSetDataForAsync(@event.AsQueryable());
            return guid;
        }

        private Guid MockEventWithOptions(bool sendEmailToManager = false)
        {
            var guid = Guid.NewGuid();

            var events = new List<Event>
            {
                new Event
                {
                    StartDate = DateTime.Parse("2016-04-05"),
                    EndDate = DateTime.Parse("2016-04-07"),
                    RegistrationDeadline = DateTime.Parse("2016-04-05"),
                    EventOptions = new List<EventOption>
                    {
                        new EventOption
                        {
                            Id = 1,
                            EventId = guid,
                            Option = "Option1"
                        },
                        new EventOption
                        {
                            Id = 2,
                            EventId = guid,
                            Option = "Option2"
                        },
                        new EventOption
                        {
                            Id = 3,
                            EventId = guid,
                            Option = "Option3"
                        },
                        new EventOption
                        {
                            Id = 4,
                            EventId = guid,
                            Option = "Option4",
                            Rule = OptionRules.IgnoreSingleJoin
                        }
                    },
                    EventQuestions = new List<EventQuestion>(),
                    Id = guid,
                    MaxChoices = 1,
                    MaxParticipants = 20,
                    OrganizationId = 2,
                    Name = "Test event",
                    EventType = new EventType
                    {
                        Name = "test type",
                        IsSingleJoin = false,
                        Id = 1,
                        SendEmailToManager = sendEmailToManager
                    },
                    EventTypeId = 1
                }
            };

            var defaultEvent = events.First();

            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    Event = defaultEvent,
                    EventOptions = new List<EventOption>(),
                    EventId = defaultEvent.Id,
                    ApplicationUserId = "user",
                    ApplicationUser = new ApplicationUser
                    {
                        Id = "user",
                        Email = "user",
                        ManagerId = "user2"
                    },
                    AttendStatus = 1
                },
                new EventParticipant
                {
                    Id = 2,
                    Event = defaultEvent,
                    EventOptions = new List<EventOption>(),
                    EventId = defaultEvent.Id,
                    ApplicationUserId = "user2",
                    ApplicationUser = new ApplicationUser
                    {
                        Id = "user2",
                        Email = "user2",
                        ManagerId = "user"
                    },
                    AttendStatus = 1
                }
            };

            foreach (var @event in events)
            {
                @event.EventParticipants = participants;
            }

            _eventParticipantsDbSet.SetDbSetDataForAsync(participants.AsQueryable());
            _eventsDbSet.SetDbSetDataForAsync(events.AsQueryable());
            return guid;
        }

        /// <summary>
        /// q5 "Pick your dish" (required, single, always shown): 90 Pasta, 91 Pizza.
        /// q6 "Which pizza?"   (required, single, shown if 91):   92 Margherita.
        /// MaxChoices is 0 — every option belongs to a question, so nothing counts as a legacy choice.
        /// </summary>
        [Test]
        public async Task Should_Keep_Stored_Answers_When_Changing_Options_Without_Any()
        {
            var eventId = MockEventWithRequiredQuestion(withAttendingUser1: true, user1AnswerIds: new[] { 90 });

            // /Events/Options serves only legacy options, so a client echoing it back sends no
            // answers. Replacing the whole selection with that would delete them.
            var dto = new EventChangeOptionsDto
            {
                EventId = eventId,
                OrganizationId = 2,
                UserId = "user1",
                ChosenOptions = new List<int>()
            };

            await _eventParticipationService.UpdateSelectedOptionsAsync(dto);

            var participant = ((IQueryable<EventParticipant>)_eventParticipantsDbSet)
                .Single(p => p.ApplicationUserId == "user1");
            Assert.That(participant.EventOptions.Select(option => option.Id), Is.EquivalentTo(new[] { 90 }));
        }

        [Test]
        public async Task Should_Keep_Stored_Answers_When_A_Status_Change_Carries_Only_Flat_Options()
        {
            // What the shipped client posts: attendStatus and nothing else. Replacing the whole
            // selection with that would delete answers the client never knew about.
            var eventId = MockEventWithRequiredQuestion(
                withAttendingUser1: true,
                user1Status: AttendingStatus.MaybeAttending,
                user1AnswerIds: new[] { 90 });

            var updateAttendStatusDto = new UpdateAttendStatusDto
            {
                EventId = eventId,
                UserId = "user1",
                OrganizationId = 2,
                AttendStatus = AttendingStatus.Attending,
                ChosenOptions = new List<int>()
            };

            await _eventParticipationService.UpdateAttendStatusAsync(updateAttendStatusDto);

            var participant = ((IQueryable<EventParticipant>)_eventParticipantsDbSet)
                .Single(p => p.ApplicationUserId == "user1");
            Assert.That(participant.EventOptions.Select(option => option.Id), Is.EquivalentTo(new[] { 90 }));
        }

        [Test]
        public void Should_Not_Block_Going_When_A_Required_Question_Was_Added_After_The_Participant_Answered()
        {
            // A host can add a required question to a live event. The shipped client cannot supply
            // an answer on a status change, so enforcing the stored ones would strand the
            // participant on Maybe with no way forward.
            var eventId = MockEventWithRequiredQuestion(
                withAttendingUser1: true,
                user1Status: AttendingStatus.MaybeAttending);

            var updateAttendStatusDto = new UpdateAttendStatusDto
            {
                EventId = eventId,
                UserId = "user1",
                OrganizationId = 2,
                AttendStatus = AttendingStatus.Attending
            };

            Assert.DoesNotThrowAsync(
                async () => await _eventParticipationService.UpdateAttendStatusAsync(updateAttendStatusDto));
        }

        [Test]
        public void Should_Reject_A_Status_Change_To_Going_That_Skips_A_Required_Question()
        {
            var eventId = MockEventWithRequiredQuestion(withAttendingUser1: true, user1Status: AttendingStatus.MaybeAttending);

            var updateAttendStatusDto = new UpdateAttendStatusDto
            {
                EventId = eventId,
                UserId = "user1",
                OrganizationId = 2,
                AttendStatus = AttendingStatus.Attending,
                ChosenOptions = new List<int>(),

                // Present but empty: the caller is submitting "no answers", which is what puts the
                // answer rules in play. Omitting it would mean "keep my stored answers".
                Answers = new List<int>()
            };

            var ex = Assert.ThrowsAsync<EventAnswersInvalidException>(
                async () => await _eventParticipationService.UpdateAttendStatusAsync(updateAttendStatusDto));

            Assert.That(ex.Errors.Select(e => e.Reason),
                Is.EquivalentTo(new[] { EventAnswerErrorReason.RequiredAnswerMissing }));
        }

        [Test]
        public async Task Should_Save_The_Answers_Carried_By_A_Status_Change_To_Going()
        {
            var eventId = MockEventWithRequiredQuestion(withAttendingUser1: true, user1Status: AttendingStatus.MaybeAttending);

            var updateAttendStatusDto = new UpdateAttendStatusDto
            {
                EventId = eventId,
                UserId = "user1",
                OrganizationId = 2,
                AttendStatus = AttendingStatus.Attending,
                ChosenOptions = new List<int> { 90 }
            };

            await _eventParticipationService.UpdateAttendStatusAsync(updateAttendStatusDto);

            var participant = ((IQueryable<EventParticipant>)_eventParticipantsDbSet)
                .Single(p => p.ApplicationUserId == "user1");
            Assert.That(participant.AttendStatus, Is.EqualTo((int)AttendingStatus.Attending));
            Assert.That(participant.EventOptions.Select(option => option.Id), Is.EquivalentTo(new[] { 90 }));
        }

        [Test]
        public void Should_Not_Validate_Answers_When_The_New_Status_Is_Not_Going()
        {
            var eventId = MockEventWithRequiredQuestion(withAttendingUser1: true, user1Status: AttendingStatus.Attending);

            var updateAttendStatusDto = new UpdateAttendStatusDto
            {
                EventId = eventId,
                UserId = "user1",
                OrganizationId = 2,
                AttendStatus = AttendingStatus.MaybeAttending,
                ChosenOptions = new List<int>()
            };

            Assert.DoesNotThrowAsync(async () => await _eventParticipationService.UpdateAttendStatusAsync(updateAttendStatusDto));
        }

        [Test]
        public async Task Should_Keep_Stored_Answers_When_A_Status_Change_Carries_None()
        {
            var eventId = MockEventWithRequiredQuestion(withAttendingUser1: true, user1Status: AttendingStatus.MaybeAttending, user1AnswerIds: new[] { 90 });

            // The shipped client posts updateAttendStatus(attendStatus, attendComment, eventId), so
            // ChosenOptions arrives empty. Falling back to the stored answer is what lets a user
            // switch Maybe back to Going without re-answering.
            var updateAttendStatusDto = new UpdateAttendStatusDto
            {
                EventId = eventId,
                UserId = "user1",
                OrganizationId = 2,
                AttendStatus = AttendingStatus.Attending,
                ChosenOptions = new List<int>()
            };

            await _eventParticipationService.UpdateAttendStatusAsync(updateAttendStatusDto);

            var participant = ((IQueryable<EventParticipant>)_eventParticipantsDbSet)
                .Single(p => p.ApplicationUserId == "user1");
            Assert.That(participant.AttendStatus, Is.EqualTo((int)AttendingStatus.Attending));
            Assert.That(participant.EventOptions.Select(option => option.Id), Is.EquivalentTo(new[] { 90 }),
                "an omitted ChosenOptions must not wipe the stored answer");
        }

        private Guid MockEventWithRequiredQuestion(bool withAttendingUser1 = false, AttendingStatus user1Status = AttendingStatus.Attending, int[] user1AnswerIds = null)
        {
            var eventId = Guid.NewGuid();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2026-01-01"));

            var dishOptions = new List<EventOption>
            {
                new EventOption { Id = 90, EventId = eventId, Option = "Pasta", QuestionId = 5, Order = 0 },
                new EventOption { Id = 91, EventId = eventId, Option = "Pizza", QuestionId = 5, Order = 1 }
            };

            var pizzaOptions = new List<EventOption>
            {
                new EventOption { Id = 92, EventId = eventId, Option = "Margherita", QuestionId = 6, Order = 0 }
            };

            var participants = withAttendingUser1
                ? new List<EventParticipant>
                {
                    new EventParticipant
                    {
                        Id = 1,
                        EventId = eventId,
                        ApplicationUserId = "user1",
                        AttendStatus = (int)user1Status,
                        EventOptions = dishOptions.Concat(pizzaOptions)
                            .Where(option => (user1AnswerIds ?? Array.Empty<int>()).Contains(option.Id))
                            .ToList()
                    }
                }
                : new List<EventParticipant>();

            var events = new List<Event>
            {
                new Event
                {
                    Id = eventId,
                    OrganizationId = 2,
                    Name = "Question event",
                    MaxChoices = 0,
                    MaxParticipants = 20,
                    MaxVirtualParticipants = 0,
                    StartDate = DateTime.Parse("2026-06-01"),
                    EndDate = DateTime.Parse("2026-06-02"),
                    RegistrationDeadline = DateTime.Parse("2026-05-01"),
                    AllowMaybeGoing = true,
                    AllowNotGoing = true,
                    ResponsibleUserId = "host1",
                    EventTypeId = 1,
                    EventType = new EventType { Id = 1, Name = "test type", IsSingleJoin = false },
                    EventParticipants = participants,
                    EventOptions = dishOptions.Concat(pizzaOptions).ToList(),
                    EventQuestions = new List<EventQuestion>
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
                            Options = dishOptions
                        },
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
                        }
                    }
                }
            };

            _eventsDbSet.SetDbSetDataForAsync(events.AsQueryable());
            _eventParticipantsDbSet.SetDbSetDataForAsync(participants.AsQueryable());
            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", OrganizationId = 2 },
                new ApplicationUser { Id = "user2", OrganizationId = 2 },
                new ApplicationUser { Id = "host1", OrganizationId = 2 }
            });

            return eventId;
        }

        /// <summary>
        /// Mixed event: one legacy option (10, no QuestionId) carrying <see cref="OptionRules.IgnoreSingleJoin"/>,
        /// plus a required question (5) with its own option (90). MaxChoices is 1, counting the legacy
        /// option only. Exercises FIX 1: question answers must not be counted by
        /// CheckIfSingleChoiceSelectedWithRule alongside the legacy IgnoreSingleJoin option.
        /// </summary>
        private Guid MockEventWithLegacyIgnoreSingleJoinOptionAndQuestion()
        {
            var eventId = Guid.NewGuid();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2026-01-01"));

            var legacyOptions = new List<EventOption>
            {
                new EventOption { Id = 10, EventId = eventId, Option = "Legacy", QuestionId = null, Rule = OptionRules.IgnoreSingleJoin, Order = 0 }
            };

            var questionOptions = new List<EventOption>
            {
                new EventOption { Id = 90, EventId = eventId, Option = "QOption", QuestionId = 5, Order = 0 }
            };

            var events = new List<Event>
            {
                new Event
                {
                    Id = eventId,
                    OrganizationId = 2,
                    Name = "Mixed event",
                    MaxChoices = 1,
                    MaxParticipants = 20,
                    MaxVirtualParticipants = 0,
                    StartDate = DateTime.Parse("2026-06-01"),
                    EndDate = DateTime.Parse("2026-06-02"),
                    RegistrationDeadline = DateTime.Parse("2026-05-01"),
                    AllowMaybeGoing = true,
                    AllowNotGoing = true,
                    ResponsibleUserId = "host1",
                    EventTypeId = 1,
                    EventType = new EventType { Id = 1, Name = "test type", IsSingleJoin = false },
                    EventParticipants = new List<EventParticipant>(),
                    EventOptions = legacyOptions.Concat(questionOptions).ToList(),
                    EventQuestions = new List<EventQuestion>
                    {
                        new EventQuestion
                        {
                            Id = 5,
                            EventId = eventId,
                            Title = "Pick one",
                            Order = 0,
                            SelectType = EventQuestionSelectType.Single,
                            IsRequired = true,
                            ShowIfOptionId = null,
                            Options = questionOptions
                        }
                    }
                }
            };

            _eventsDbSet.SetDbSetDataForAsync(events.AsQueryable());
            _eventParticipantsDbSet.SetDbSetDataForAsync(new List<EventParticipant>().AsQueryable());
            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", OrganizationId = 2 }
            });

            return eventId;
        }

        private void MockUsers()
        {
            var types = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "1",
                    OrganizationId = 2,
                    ManagerId = "testUserId",
                    Email = "1@1.com"
                },
                new ApplicationUser
                {
                    Id = "testUserId",
                    OrganizationId = 2,
                    ManagerId = "1",
                    Email = "testUserId@testUserId.com"
                }
            };
            _usersDbSet.SetDbSetDataForAsync(types.AsQueryable());
        }

        #endregion
    }
}
