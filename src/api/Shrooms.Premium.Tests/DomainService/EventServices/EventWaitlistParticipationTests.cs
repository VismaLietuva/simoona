using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
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
using Shrooms.Premium.Domain.Services.Events.Calendar;
using Shrooms.Premium.Domain.Services.Events.Participation;
using Shrooms.Premium.Tests.Mocks;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    [TestFixture]
    public class EventWaitlistParticipationTests
    {
        private DbSet<Event> _eventsDbSet;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<EventOption> _eventOptionsDbSet;
        private DbSet<EventParticipant> _eventParticipantsDbSet;

        private IUnitOfWork2 _uow;
        private ISystemClock _systemClockMock;
        private IPermissionService _permissionService;
        private IWallService _wallService;
        private IAsyncRunner _asyncRunner;
        private EventParticipationService _sut;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();

            _eventsDbSet = Substitute.For<DbSet<Event>, IQueryable<Event>, IAsyncEnumerable<Event>>();
            _uow.GetDbSet<Event>().Returns(_eventsDbSet);

            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IAsyncEnumerable<ApplicationUser>>();
            _uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            _eventParticipantsDbSet = Substitute.For<DbSet<EventParticipant>, IQueryable<EventParticipant>, IAsyncEnumerable<EventParticipant>>();
            _uow.GetDbSet<EventParticipant>().Returns(_eventParticipantsDbSet);

            _eventOptionsDbSet = Substitute.For<DbSet<EventOption>, IQueryable<EventOption>, IAsyncEnumerable<EventOption>>();
            _uow.GetDbSet<EventOption>().Returns(_eventOptionsDbSet);

            _systemClockMock = Substitute.For<ISystemClock>();
            _permissionService = Substitute.For<IPermissionService>();
            _wallService = Substitute.For<IWallService>();
            _asyncRunner = Substitute.For<IAsyncRunner>();

            _sut = new EventParticipationService(
                _uow,
                _systemClockMock,
                Substitute.For<IRoleService>(),
                _permissionService,
                new EventValidationService(_systemClockMock),
                _wallService,
                _asyncRunner,
                new EventAnswerValidator(),
                new PassThroughEventSeatLock());
        }

        [Test]
        public async Task Should_Waitlist_The_Joiner_When_The_Event_Is_Full_And_Has_A_Waitlist()
        {
            var eventGuid = MockFullEvent(hasWaitlist: true);
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            await _sut.JoinAsync(JoinDto(eventGuid));

            _eventParticipantsDbSet.Received(1).Add(Arg.Is<EventParticipant>(p =>
                p.ApplicationUserId == "testUserId" &&
                p.AttendStatus == (int)AttendingStatus.Waitlisted &&
                p.WaitlistedAt == DateTime.Parse("2016-03-28")));
        }

        [Test]
        public void Should_Still_Throw_Full_When_The_Event_Is_Full_Without_A_Waitlist()
        {
            var eventGuid = MockFullEvent(hasWaitlist: false);
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            var exception = Assert.ThrowsAsync<EventException>(() => _sut.JoinAsync(JoinDto(eventGuid)));

            Assert.That(exception.Message, Is.EqualTo(PremiumErrorCodes.EventIsFullCode));
        }

        [Test]
        public async Task Should_Not_Waitlist_When_There_Is_A_Free_Seat()
        {
            var eventGuid = MockFullEvent(hasWaitlist: true, maxParticipants: 2);
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            await _sut.JoinAsync(JoinDto(eventGuid));

            _eventParticipantsDbSet.Received(1).Add(Arg.Is<EventParticipant>(p => p.AttendStatus == (int)AttendingStatus.Attending));
        }

        [Test]
        public void Should_Not_Waitlist_A_Colleague_Added_By_The_Host()
        {
            var eventGuid = MockFullEvent(hasWaitlist: true);
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));
            _permissionService.UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Event).Returns(true);

            var exception = Assert.ThrowsAsync<EventException>(() => _sut.AddColleagueAsync(JoinDto(eventGuid)));

            Assert.That(exception.Message, Is.EqualTo(PremiumErrorCodes.EventIsFullCode));
        }

        [Test]
        public async Task Should_Not_Send_An_Invite_Or_Manager_Email_For_A_Waitlisted_Joiner()
        {
            var eventGuid = MockFullEvent(hasWaitlist: true, sendEmailToManager: true);
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            await _sut.JoinAsync(JoinDto(eventGuid));

            _asyncRunner.DidNotReceiveWithAnyArgs().Run<IEventCalendarService>(default, default);
            _asyncRunner.DidNotReceiveWithAnyArgs().Run<IEventNotificationService>(default, default);
        }

        [Test]
        public async Task Should_Join_The_Event_Wall_When_Waitlisted()
        {
            var eventGuid = MockFullEvent(hasWaitlist: true);
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            await _sut.JoinAsync(JoinDto(eventGuid));

            await _wallService.Received(1).JoinOrLeaveWallAsync(Arg.Any<int>(), "testUserId", "testUserId", 2, true);
        }

        [Test]
        public async Task Should_Keep_The_Queue_Position_When_A_Waitlisted_User_Joins_Again()
        {
            var eventGuid = MockFullEvent(hasWaitlist: true, queuedUserId: "testUserId", queuedAt: DateTime.Parse("2016-03-20"));
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            await _sut.JoinAsync(JoinDto(eventGuid));

            var queued = _eventParticipantsDbSet.ToList().Single(p => p.ApplicationUserId == "testUserId");
            Assert.That(queued.AttendStatus, Is.EqualTo((int)AttendingStatus.Waitlisted));
            Assert.That(queued.WaitlistedAt, Is.EqualTo(DateTime.Parse("2016-03-20")));
        }

        [Test]
        public void Should_Reject_Maybe_To_Going_On_A_Full_Event_Without_A_Waitlist()
        {
            var eventGuid = MockFullEvent(hasWaitlist: false, maybeUserId: "testUserId");
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            var exception = Assert.ThrowsAsync<EventException>(() => _sut.UpdateAttendStatusAsync(UpdateDto(eventGuid, AttendingStatus.Attending)));

            Assert.That(exception.Message, Is.EqualTo(PremiumErrorCodes.EventIsFullCode));
        }

        [Test]
        public async Task Should_Waitlist_Maybe_To_Going_On_A_Full_Waitlist_Event()
        {
            var eventGuid = MockFullEvent(hasWaitlist: true, maybeUserId: "testUserId");
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            await _sut.UpdateAttendStatusAsync(UpdateDto(eventGuid, AttendingStatus.Attending));

            var participant = _eventParticipantsDbSet.ToList().Single(p => p.ApplicationUserId == "testUserId");
            Assert.That(participant.AttendStatus, Is.EqualTo((int)AttendingStatus.Waitlisted));
            Assert.That(participant.WaitlistedAt, Is.EqualTo(DateTime.Parse("2016-03-28")));
        }

        [Test]
        public async Task Should_Leave_The_Queue_When_A_Waitlisted_User_Chooses_Maybe()
        {
            var eventGuid = MockFullEvent(hasWaitlist: true, queuedUserId: "testUserId", queuedAt: DateTime.Parse("2016-03-20"));
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            await _sut.UpdateAttendStatusAsync(UpdateDto(eventGuid, AttendingStatus.MaybeAttending));

            var participant = _eventParticipantsDbSet.ToList().Single(p => p.ApplicationUserId == "testUserId");
            Assert.That(participant.AttendStatus, Is.EqualTo((int)AttendingStatus.MaybeAttending));
            Assert.That(participant.WaitlistedAt, Is.Null);
        }

        [Test]
        public async Task Should_Keep_Position_When_A_Queued_User_Asks_For_Going_While_Still_Full()
        {
            var eventGuid = MockFullEvent(hasWaitlist: true, queuedUserId: "testUserId", queuedAt: DateTime.Parse("2016-03-20"));
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            await _sut.UpdateAttendStatusAsync(UpdateDto(eventGuid, AttendingStatus.Attending));

            var participant = _eventParticipantsDbSet.ToList().Single(p => p.ApplicationUserId == "testUserId");
            Assert.That(participant.AttendStatus, Is.EqualTo((int)AttendingStatus.Waitlisted));
            Assert.That(participant.WaitlistedAt, Is.EqualTo(DateTime.Parse("2016-03-20")));
        }

        [Test]
        public async Task Should_Allow_Going_When_A_Seat_Is_Free()
        {
            var eventGuid = MockFullEvent(hasWaitlist: false, maxParticipants: 2, maybeUserId: "testUserId");
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

            await _sut.UpdateAttendStatusAsync(UpdateDto(eventGuid, AttendingStatus.Attending));

            var participant = _eventParticipantsDbSet.ToList().Single(p => p.ApplicationUserId == "testUserId");
            Assert.That(participant.AttendStatus, Is.EqualTo((int)AttendingStatus.Attending));
        }

        private static UpdateAttendStatusDto UpdateDto(Guid eventId, AttendingStatus status)
        {
            return new UpdateAttendStatusDto
            {
                EventId = eventId,
                UserId = "testUserId",
                OrganizationId = 2,
                AttendStatus = status,
                AttendComment = string.Empty
            };
        }

        private static EventJoinDto JoinDto(Guid eventGuid)
        {
            return new EventJoinDto
            {
                ChosenOptions = new List<int>(),
                ParticipantIds = new List<string> { "testUserId" },
                AttendStatus = AttendingStatus.Attending,
                EventId = eventGuid,
                UserId = "testUserId",
                OrganizationId = 2
            };
        }

        private Guid MockFullEvent(
            bool hasWaitlist,
            int maxParticipants = 1,
            bool sendEmailToManager = false,
            string queuedUserId = null,
            DateTime? queuedAt = null,
            string maybeUserId = null)
        {
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                StartDate = DateTime.Parse("2016-04-05"),
                EndDate = DateTime.Parse("2016-04-06"),
                RegistrationDeadline = DateTime.Parse("2016-04-05"),
                EventOptions = new List<EventOption>(),
                EventQuestions = new List<EventQuestion>(),
                Id = eventId,
                MaxChoices = 0,
                MaxParticipants = maxParticipants,
                HasWaitlist = hasWaitlist,
                AllowMaybeGoing = true,
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
            };

            var participants = new List<EventParticipant>
            {
                CreateParticipant(@event, "otherUser", AttendingStatus.Attending, null)
            };

            if (queuedUserId != null)
            {
                participants.Add(CreateParticipant(@event, queuedUserId, AttendingStatus.Waitlisted, queuedAt));
            }

            if (maybeUserId != null)
            {
                participants.Add(CreateParticipant(@event, maybeUserId, AttendingStatus.MaybeAttending, null));
            }

            foreach (var participant in participants)
            {
                @event.EventParticipants.Add(participant);
            }

            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event });
            MockParticipantsDbSet(participants);
            return eventId;
        }

        private static EventParticipant CreateParticipant(Event @event, string userId, AttendingStatus status, DateTime? waitlistedAt)
        {
            return new EventParticipant
            {
                Event = @event,
                EventId = @event.Id,
                ApplicationUserId = userId,
                AttendStatus = (int)status,
                WaitlistedAt = waitlistedAt,
                EventOptions = new List<EventOption>()
            };
        }

        private void MockParticipantsDbSet(List<EventParticipant> participants)
        {
            _eventParticipantsDbSet.SetDbSetDataForAsync(participants.ToList());
            _eventParticipantsDbSet
                .When(dbSet => dbSet.Add(Arg.Any<EventParticipant>()))
                .Do(callInfo =>
                {
                    participants.Add(callInfo.Arg<EventParticipant>());
                    _eventParticipantsDbSet.SetDbSetDataForAsync(participants.ToList());
                });
        }

        private void MockUsers()
        {
            var users = new List<ApplicationUser>
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
            _usersDbSet.SetDbSetDataForAsync(users);
        }
    }
}
