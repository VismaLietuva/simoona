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
using Shrooms.Contracts.DataTransferObjects.Users;
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
        private List<EventParticipant> _participants;
        private Event _event;

        private IUnitOfWork2 _uow;
        private ISystemClock _systemClockMock;
        private IPermissionService _permissionService;
        private IWallService _wallService;
        private IAsyncRunner _asyncRunner;
        private EventParticipationService _sut;

        private static readonly DateTime Start = DateTime.Parse("2016-04-10");

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
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));

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

        [Test]
        public async Task Should_Promote_The_Earliest_Queued_User_When_A_Seat_Frees_Up()
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("going", AttendingStatus.Attending, null),
                ("second", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-21")),
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));
            RemoveParticipant("going");

            await _sut.PromoteFromWaitlistAsync(eventGuid, 2);

            Assert.That(StatusOf("first"), Is.EqualTo(AttendingStatus.Attending));
            Assert.That(StatusOf("second"), Is.EqualTo(AttendingStatus.Waitlisted));
            Assert.That(WaitlistedAtOf("first"), Is.Null);
            await _uow.Received().SaveChangesAsync(false);
        }

        [Test]
        public async Task Should_Order_The_Queue_By_Id_When_Users_Queued_At_The_Same_Time()
        {
            var queuedAt = DateTime.Parse("2016-03-20");
            var eventGuid = MockWaitlistEvent(1, Start,
                ("later", AttendingStatus.Waitlisted, queuedAt),
                ("earlier", AttendingStatus.Waitlisted, queuedAt));
            _participants.Single(p => p.ApplicationUserId == "later").Id = 2;
            _participants.Single(p => p.ApplicationUserId == "earlier").Id = 1;

            await _sut.PromoteFromWaitlistAsync(eventGuid, 2);

            Assert.That(StatusOf("earlier"), Is.EqualTo(AttendingStatus.Attending));
            Assert.That(StatusOf("later"), Is.EqualTo(AttendingStatus.Waitlisted));
        }

        [Test]
        public async Task Should_Promote_Several_When_Several_Seats_Are_Free()
        {
            var eventGuid = MockWaitlistEvent(3, Start,
                ("going", AttendingStatus.Attending, null),
                ("a", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")),
                ("b", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-21")),
                ("c", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-22")));

            await _sut.PromoteFromWaitlistAsync(eventGuid, 2);

            Assert.That(StatusOf("a"), Is.EqualTo(AttendingStatus.Attending));
            Assert.That(StatusOf("b"), Is.EqualTo(AttendingStatus.Attending));
            Assert.That(StatusOf("c"), Is.EqualTo(AttendingStatus.Waitlisted));
        }

        [Test]
        public async Task Should_Not_Promote_When_The_Event_Is_Still_Full()
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("going", AttendingStatus.Attending, null),
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));

            await _sut.PromoteFromWaitlistAsync(eventGuid, 2);

            Assert.That(StatusOf("first"), Is.EqualTo(AttendingStatus.Waitlisted));
        }

        [Test]
        public async Task Should_Not_Promote_Within_The_Cutoff_Before_The_Start()
        {
            _systemClockMock.UtcNow.Returns(Start.AddHours(-EventsConstants.WaitlistPromotionCutoffHours).AddMinutes(1));
            var eventGuid = MockWaitlistEvent(1, Start,
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));

            await _sut.PromoteFromWaitlistAsync(eventGuid, 2);

            Assert.That(StatusOf("first"), Is.EqualTo(AttendingStatus.Waitlisted));
        }

        [Test]
        public async Task Should_Not_Promote_Exactly_At_The_Cutoff()
        {
            _systemClockMock.UtcNow.Returns(Start.AddHours(-EventsConstants.WaitlistPromotionCutoffHours));
            var eventGuid = MockWaitlistEvent(1, Start,
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));

            await _sut.PromoteFromWaitlistAsync(eventGuid, 2);

            Assert.That(StatusOf("first"), Is.EqualTo(AttendingStatus.Waitlisted));
        }

        [Test]
        public async Task Should_Promote_Just_Before_The_Cutoff()
        {
            _systemClockMock.UtcNow.Returns(Start.AddHours(-EventsConstants.WaitlistPromotionCutoffHours).AddMinutes(-1));
            var eventGuid = MockWaitlistEvent(1, Start,
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));

            await _sut.PromoteFromWaitlistAsync(eventGuid, 2);

            Assert.That(StatusOf("first"), Is.EqualTo(AttendingStatus.Attending));
        }

        [Test]
        public async Task Should_Not_Promote_On_An_Event_Without_A_Waitlist()
        {
            var eventGuid = MockWaitlistEvent(1, Start, false,
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));

            await _sut.PromoteFromWaitlistAsync(eventGuid, 2);

            Assert.That(StatusOf("first"), Is.EqualTo(AttendingStatus.Waitlisted));
        }

        [Test]
        public async Task Should_Not_Promote_An_Event_Of_Another_Organization()
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));

            await _sut.PromoteFromWaitlistAsync(eventGuid, 3);

            Assert.That(StatusOf("first"), Is.EqualTo(AttendingStatus.Waitlisted));
        }

        [Test]
        public async Task Should_Notify_Promoted_Users_And_Send_The_Calendar_Invite()
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));

            await _sut.PromoteFromWaitlistAsync(eventGuid, 2);

            _asyncRunner.Received(1).Run<IEventNotificationService>(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
            _asyncRunner.Received(1).Run<IEventCalendarService>(Arg.Any<Func<IEventCalendarService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Send_Nothing_When_Nobody_Is_Promoted()
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("going", AttendingStatus.Attending, null),
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));

            await _sut.PromoteFromWaitlistAsync(eventGuid, 2);

            _asyncRunner.DidNotReceiveWithAnyArgs().Run<IEventNotificationService>(default, default);
            _asyncRunner.DidNotReceiveWithAnyArgs().Run<IEventCalendarService>(default, default);
            await _uow.DidNotReceiveWithAnyArgs().SaveChangesAsync(default(bool));
        }

        [Test]
        public async Task Should_Skip_A_Candidate_Who_Breaks_The_Single_Join_Rule()
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("blocked", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")),
                ("next", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-21")));
            MakeSingleJoinConflictFor("blocked");

            await _sut.PromoteFromWaitlistAsync(eventGuid, 2);

            Assert.That(StatusOf("blocked"), Is.EqualTo(AttendingStatus.Waitlisted));
            Assert.That(StatusOf("next"), Is.EqualTo(AttendingStatus.Attending));
        }

        [Test]
        public async Task Should_Keep_The_Queue_Position_Of_A_Skipped_Candidate()
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("blocked", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));
            MakeSingleJoinConflictFor("blocked");

            await _sut.PromoteFromWaitlistAsync(eventGuid, 2);

            Assert.That(WaitlistedAtOf("blocked"), Is.EqualTo(DateTime.Parse("2016-03-20")));
            _asyncRunner.DidNotReceiveWithAnyArgs().Run<IEventNotificationService>(default, default);
        }

        [Test]
        public async Task Should_Promote_After_An_Attending_User_Leaves()
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("going", AttendingStatus.Attending, null),
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));

            await _sut.LeaveAsync(eventGuid, new UserAndOrganizationDto { UserId = "going", OrganizationId = 2 }, string.Empty);

            Assert.That(StatusOf("first"), Is.EqualTo(AttendingStatus.Attending));
        }

        [Test]
        public async Task Should_Not_Promote_When_A_Waitlisted_User_Leaves()
        {
            var eventGuid = MockWaitlistEvent(2, Start,
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")),
                ("second", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-21")));

            await _sut.LeaveAsync(eventGuid, new UserAndOrganizationDto { UserId = "first", OrganizationId = 2 }, string.Empty);

            Assert.That(StatusOf("second"), Is.EqualTo(AttendingStatus.Waitlisted));
        }

        [Test]
        public async Task Should_Promote_After_An_Attending_User_Is_Expelled_And_Still_Notify_The_Expelled_User()
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("going", AttendingStatus.Attending, null),
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));
            _permissionService.UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Event).Returns(true);

            await _sut.ExpelAsync(eventGuid, new UserAndOrganizationDto { UserId = "host", OrganizationId = 2 }, "going");

            Assert.That(StatusOf("first"), Is.EqualTo(AttendingStatus.Attending));
            _asyncRunner.Received(2).Run<IEventNotificationService>(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Not_Promote_When_A_Waitlisted_User_Is_Expelled()
        {
            var eventGuid = MockWaitlistEvent(2, Start,
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")),
                ("second", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-21")));
            _permissionService.UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Event).Returns(true);

            await _sut.ExpelAsync(eventGuid, new UserAndOrganizationDto { UserId = "host", OrganizationId = 2 }, "first");

            Assert.That(StatusOf("second"), Is.EqualTo(AttendingStatus.Waitlisted));
            _asyncRunner.Received(1).Run<IEventNotificationService>(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Promote_After_An_Attending_User_Changes_To_Not_Going()
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("testUserId", AttendingStatus.Attending, null),
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));

            await _sut.UpdateAttendStatusAsync(UpdateDto(eventGuid, AttendingStatus.NotAttending));

            Assert.That(StatusOf("first"), Is.EqualTo(AttendingStatus.Attending));
        }

        [Test]
        public async Task Should_Not_Promote_When_A_Maybe_User_Changes_To_Not_Going()
        {
            var eventGuid = MockWaitlistEvent(2, Start,
                ("going", AttendingStatus.Attending, null),
                ("testUserId", AttendingStatus.MaybeAttending, null),
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));

            await _sut.UpdateAttendStatusAsync(UpdateDto(eventGuid, AttendingStatus.NotAttending));

            Assert.That(StatusOf("first"), Is.EqualTo(AttendingStatus.Waitlisted));
        }

        [Test]
        public async Task Should_Leave_Waitlisted_People_Off_The_Roster()
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("going", AttendingStatus.Attending, null),
                ("queued", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));
            SetNames("going", "Going", "Person");
            SetNames("queued", "Queued", "Person");

            var roster = (await _sut.GetEventParticipantsAsync(eventGuid, new UserAndOrganizationDto { UserId = "host", OrganizationId = 2 })).ToList();

            Assert.That(roster.Select(p => p.FirstName), Is.EqualTo(new[] { "Going" }));
        }

        [TestCase("2016-03-27", AttendingStatus.Waitlisted)]
        [TestCase("2016-03-28", AttendingStatus.Attending)]
        [TestCase("2016-03-29", AttendingStatus.Attending)]
        public async Task Should_Promote_Only_Until_The_Registration_Deadline(string deadline, AttendingStatus expected)
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));
            _event.RegistrationDeadline = DateTime.Parse(deadline);

            await _sut.PromoteFromWaitlistAsync(eventGuid, 2);

            Assert.That(StatusOf("first"), Is.EqualTo(expected));
        }

        [Test]
        public async Task Should_Free_And_Fill_A_Seat_On_Leave_In_One_Lock_And_Notify_After_It()
        {
            var seatLock = UseRecordingSeatLock();
            var eventGuid = MockWaitlistEvent(1, Start,
                ("going", AttendingStatus.Attending, null),
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));

            await _sut.LeaveAsync(eventGuid, new UserAndOrganizationDto { UserId = "going", OrganizationId = 2 }, string.Empty);

            Assert.That(seatLock.Calls, Is.EqualTo(1));
            Assert.That(seatLock.DispatchedInsideLock, Is.False);
            Assert.That(seatLock.SavedOutsideLock, Is.False);
            Assert.That(StatusOf("first"), Is.EqualTo(AttendingStatus.Attending));
            _asyncRunner.Received(1).Run<IEventCalendarService>(Arg.Any<Func<IEventCalendarService, Task>>(), Arg.Any<string>());
            _asyncRunner.Received(1).Run<IEventNotificationService>(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Free_And_Fill_A_Seat_On_Expel_In_One_Lock_And_Notify_After_It()
        {
            var seatLock = UseRecordingSeatLock();
            var eventGuid = MockWaitlistEvent(1, Start,
                ("going", AttendingStatus.Attending, null),
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));
            _permissionService.UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Event).Returns(true);

            await _sut.ExpelAsync(eventGuid, new UserAndOrganizationDto { UserId = "host", OrganizationId = 2 }, "going");

            Assert.That(seatLock.Calls, Is.EqualTo(1));
            Assert.That(seatLock.DispatchedInsideLock, Is.False);
            Assert.That(seatLock.SavedOutsideLock, Is.False);
            Assert.That(StatusOf("first"), Is.EqualTo(AttendingStatus.Attending));
            _asyncRunner.Received(1).Run<IEventCalendarService>(Arg.Any<Func<IEventCalendarService, Task>>(), Arg.Any<string>());
            _asyncRunner.Received(2).Run<IEventNotificationService>(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Free_And_Fill_A_Seat_On_A_Status_Change_In_One_Lock_And_Notify_After_It()
        {
            var seatLock = UseRecordingSeatLock();
            var eventGuid = MockWaitlistEvent(1, Start,
                ("testUserId", AttendingStatus.Attending, null),
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));

            await _sut.UpdateAttendStatusAsync(UpdateDto(eventGuid, AttendingStatus.NotAttending));

            Assert.That(seatLock.Calls, Is.EqualTo(1));
            Assert.That(seatLock.DispatchedInsideLock, Is.False);
            Assert.That(seatLock.SavedOutsideLock, Is.False);
            Assert.That(StatusOf("first"), Is.EqualTo(AttendingStatus.Attending));
            _asyncRunner.Received(1).Run<IEventCalendarService>(Arg.Any<Func<IEventCalendarService, Task>>(), Arg.Any<string>());
            _asyncRunner.Received(1).Run<IEventNotificationService>(Arg.Any<Func<IEventNotificationService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Send_The_Join_Invite_After_The_Seat_Lock_Is_Released()
        {
            var seatLock = UseRecordingSeatLock();
            var eventGuid = MockFullEvent(hasWaitlist: true, maxParticipants: 2);
            MockUsers();

            await _sut.JoinAsync(JoinDto(eventGuid));

            Assert.That(seatLock.Calls, Is.EqualTo(1));
            Assert.That(seatLock.DispatchedInsideLock, Is.False);
            Assert.That(seatLock.SavedOutsideLock, Is.False);
            _asyncRunner.Received(1).Run<IEventCalendarService>(Arg.Any<Func<IEventCalendarService, Task>>(), Arg.Any<string>());
        }

        [Test]
        public async Task Should_Keep_A_Waitlisted_User_On_The_Wall_And_Clear_The_Queue_Time_When_They_Join_Virtually()
        {
            var eventGuid = MockFullEvent(hasWaitlist: true, maxVirtualParticipants: 5, queuedUserId: "testUserId", queuedAt: DateTime.Parse("2016-03-20"));
            MockUsers();
            var joinDto = JoinDto(eventGuid);
            joinDto.AttendStatus = AttendingStatus.AttendingVirtually;

            await _sut.JoinAsync(joinDto);

            Assert.That(StatusOf("testUserId"), Is.EqualTo(AttendingStatus.AttendingVirtually));
            Assert.That(WaitlistedAtOf("testUserId"), Is.Null);
            await _wallService.DidNotReceiveWithAnyArgs().JoinOrLeaveWallAsync(default, default, default, default, default);
        }

        [Test]
        public async Task Should_Keep_A_Waitlisted_User_On_The_Wall_When_They_Join_Again()
        {
            var eventGuid = MockFullEvent(hasWaitlist: true, queuedUserId: "testUserId", queuedAt: DateTime.Parse("2016-03-20"));
            MockUsers();

            await _sut.JoinAsync(JoinDto(eventGuid));

            Assert.That(StatusOf("testUserId"), Is.EqualTo(AttendingStatus.Waitlisted));
            await _wallService.DidNotReceiveWithAnyArgs().JoinOrLeaveWallAsync(default, default, default, default, default);
        }

        [Test]
        public async Task Should_Keep_A_Waitlisted_User_On_The_Wall_And_Clear_The_Queue_Time_When_They_Join_A_Free_Seat()
        {
            var eventGuid = MockFullEvent(hasWaitlist: true, maxParticipants: 2, queuedUserId: "testUserId", queuedAt: DateTime.Parse("2016-03-20"));
            MockUsers();

            await _sut.JoinAsync(JoinDto(eventGuid));

            Assert.That(StatusOf("testUserId"), Is.EqualTo(AttendingStatus.Attending));
            Assert.That(WaitlistedAtOf("testUserId"), Is.Null);
            await _wallService.DidNotReceiveWithAnyArgs().JoinOrLeaveWallAsync(default, default, default, default, default);
        }

        [Test]
        public async Task Should_Join_The_Event_Wall_On_A_First_Time_Join()
        {
            var eventGuid = MockFullEvent(hasWaitlist: true, maxParticipants: 2);
            MockUsers();

            await _sut.JoinAsync(JoinDto(eventGuid));

            await _wallService.Received(1).JoinOrLeaveWallAsync(Arg.Any<int>(), "testUserId", "testUserId", 2, true);
        }

        [Test]
        public async Task Should_Tell_The_Manager_When_A_Promoted_User_Gets_A_Seat()
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));
            _event.EventType.SendEmailToManager = true;
            MockUserWithManager("first");
            var notifier = CaptureNotifications();

            await _sut.PromoteFromWaitlistAsync(eventGuid, 2);

            await notifier.Received(1).NotifyManagerAboutEventAsync(
                Arg.Is<UserEventAttendStatusChangeEmailDto>(dto => dto.ManagerEmail == "boss@example.com" && dto.EventId == eventGuid),
                true);
        }

        [Test]
        public async Task Should_Not_Tell_The_Manager_About_A_Promotion_When_The_Event_Type_Does_Not_Ask_For_It()
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));
            var notifier = CaptureNotifications();

            await _sut.PromoteFromWaitlistAsync(eventGuid, 2);

            await notifier.DidNotReceiveWithAnyArgs().NotifyManagerAboutEventAsync(default, default);
        }

        [Test]
        public async Task Should_Not_Tell_The_Manager_When_A_Waitlisted_User_Leaves()
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("going", AttendingStatus.Attending, null),
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));
            _event.EventType.SendEmailToManager = true;
            MockUserWithManager("first");
            var notifier = CaptureNotifications();

            await _sut.LeaveAsync(eventGuid, new UserAndOrganizationDto { UserId = "first", OrganizationId = 2 }, string.Empty);

            await notifier.DidNotReceiveWithAnyArgs().NotifyManagerAboutEventAsync(default, default);
        }

        [Test]
        public async Task Should_Still_Tell_The_Manager_When_An_Attending_User_Leaves()
        {
            var eventGuid = MockWaitlistEvent(2, Start,
                ("going", AttendingStatus.Attending, null));
            _event.EventType.SendEmailToManager = true;
            MockUserWithManager("going");
            var notifier = CaptureNotifications();

            await _sut.LeaveAsync(eventGuid, new UserAndOrganizationDto { UserId = "going", OrganizationId = 2 }, string.Empty);

            await notifier.Received(1).NotifyManagerAboutEventAsync(Arg.Is<UserEventAttendStatusChangeEmailDto>(dto => dto.ManagerEmail == "boss@example.com"), false);
        }

        [Test]
        public async Task Should_Not_Tell_The_Manager_When_A_Waitlisted_User_Is_Expelled()
        {
            var eventGuid = MockWaitlistEvent(1, Start,
                ("going", AttendingStatus.Attending, null),
                ("first", AttendingStatus.Waitlisted, DateTime.Parse("2016-03-20")));
            _event.EventType.SendEmailToManager = true;
            MockUserWithManager("first");
            _permissionService.UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Event).Returns(true);
            var notifier = CaptureNotifications();

            await _sut.ExpelAsync(eventGuid, new UserAndOrganizationDto { UserId = "host", OrganizationId = 2 }, "first");

            await notifier.DidNotReceiveWithAnyArgs().NotifyManagerAboutEventAsync(default, default);
            await notifier.Received(1).NotifyRemovedEventParticipantsAsync(Arg.Any<string>(), eventGuid, 2, Arg.Any<IEnumerable<string>>());
        }

        private RecordingEventSeatLock UseRecordingSeatLock()
        {
            var seatLock = new RecordingEventSeatLock();
            _asyncRunner
                .WhenForAnyArgs(runner => runner.Run<IEventCalendarService>(default, default))
                .Do(_ => seatLock.RecordDispatch());
            _asyncRunner
                .WhenForAnyArgs(runner => runner.Run<IEventNotificationService>(default, default))
                .Do(_ => seatLock.RecordDispatch());
            _uow
                .WhenForAnyArgs(uow => uow.SaveChangesAsync(default(bool)))
                .Do(_ => seatLock.RecordSave());

            _sut = new EventParticipationService(
                _uow,
                _systemClockMock,
                Substitute.For<IRoleService>(),
                _permissionService,
                new EventValidationService(_systemClockMock),
                _wallService,
                _asyncRunner,
                new EventAnswerValidator(),
                seatLock);

            return seatLock;
        }

        private IEventNotificationService CaptureNotifications()
        {
            var notifier = Substitute.For<IEventNotificationService>();
            _asyncRunner
                .WhenForAnyArgs(runner => runner.Run<IEventNotificationService>(default, default))
                .Do(callInfo => callInfo.Arg<Func<IEventNotificationService, Task>>()(notifier).GetAwaiter().GetResult());

            return notifier;
        }

        private void MockUserWithManager(string userId)
        {
            var manager = new ApplicationUser { Id = "boss", OrganizationId = 2, Email = "boss@example.com" };
            var user = _participants.Single(p => p.ApplicationUserId == userId).ApplicationUser;
            user.OrganizationId = 2;
            user.ManagerId = manager.Id;
            user.Manager = manager;

            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser> { user, manager });
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
            int maxVirtualParticipants = 0,
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
                MaxVirtualParticipants = maxVirtualParticipants,
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

        private Guid MockWaitlistEvent(int maxParticipants, DateTime start, params (string userId, AttendingStatus status, DateTime? queuedAt)[] participants) =>
            MockWaitlistEvent(maxParticipants, start, true, participants);

        private Guid MockWaitlistEvent(int maxParticipants, DateTime start, bool hasWaitlist, params (string userId, AttendingStatus status, DateTime? queuedAt)[] participants)
        {
            _event = new Event
            {
                StartDate = start,
                EndDate = start.AddDays(1),
                RegistrationDeadline = start,
                EventOptions = new List<EventOption>(),
                EventQuestions = new List<EventQuestion>(),
                Id = Guid.NewGuid(),
                MaxChoices = 0,
                MaxParticipants = maxParticipants,
                HasWaitlist = hasWaitlist,
                AllowMaybeGoing = true,
                AllowNotGoing = true,
                OrganizationId = 2,
                Name = "Test event",
                ResponsibleUserId = "host",
                EventType = new EventType
                {
                    Name = "test type",
                    IsSingleJoin = false,
                    Id = 1
                },
                EventParticipants = new List<EventParticipant>(),
                EventTypeId = 1
            };

            var rows = participants
                .Select(p => CreateParticipant(_event, p.userId, p.status, p.queuedAt))
                .ToList();

            foreach (var row in rows)
            {
                _event.EventParticipants.Add(row);
            }

            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { _event });
            MockParticipantsDbSet(rows);
            return _event.Id;
        }

        private AttendingStatus StatusOf(string userId) =>
            (AttendingStatus)_participants.Single(p => p.ApplicationUserId == userId).AttendStatus;

        private DateTime? WaitlistedAtOf(string userId) =>
            _participants.Single(p => p.ApplicationUserId == userId).WaitlistedAt;

        private void SetNames(string userId, string firstName, string lastName)
        {
            var user = _participants.Single(p => p.ApplicationUserId == userId).ApplicationUser;
            user.FirstName = firstName;
            user.LastName = lastName;
        }

        private void RemoveParticipant(string userId) =>
            _eventParticipantsDbSet.Remove(_participants.Single(p => p.ApplicationUserId == userId));

        private void MakeSingleJoinConflictFor(string userId)
        {
            _event.EventType.IsSingleJoin = true;

            var sameWeekEvent = new Event
            {
                Id = Guid.NewGuid(),
                OrganizationId = 2,
                Name = "Same week event",
                StartDate = _event.StartDate.AddDays(2),
                EndDate = _event.StartDate.AddDays(3),
                EventTypeId = _event.EventTypeId,
                EventType = _event.EventType,
                EventOptions = new List<EventOption>(),
                EventQuestions = new List<EventQuestion>(),
                EventParticipants = new List<EventParticipant>
                {
                    new EventParticipant
                    {
                        ApplicationUserId = userId,
                        AttendStatus = (int)AttendingStatus.Attending,
                        EventOptions = new List<EventOption>()
                    }
                }
            };

            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { _event, sameWeekEvent });
        }

        private static EventParticipant CreateParticipant(Event @event, string userId, AttendingStatus status, DateTime? waitlistedAt)
        {
            return new EventParticipant
            {
                Event = @event,
                EventId = @event.Id,
                ApplicationUserId = userId,
                ApplicationUser = new ApplicationUser { Id = userId },
                AttendStatus = (int)status,
                WaitlistedAt = waitlistedAt,
                EventOptions = new List<EventOption>()
            };
        }

        private void MockParticipantsDbSet(List<EventParticipant> participants)
        {
            _participants = participants;
            _eventParticipantsDbSet.SetDbSetDataForAsync(participants.ToList());
            _eventParticipantsDbSet
                .When(dbSet => dbSet.Remove(Arg.Any<EventParticipant>()))
                .Do(callInfo =>
                {
                    var removed = callInfo.Arg<EventParticipant>();
                    participants.Remove(removed);
                    removed.Event?.EventParticipants.Remove(removed);
                    _eventParticipantsDbSet.SetDbSetDataForAsync(participants.ToList());
                });
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

        private sealed class RecordingEventSeatLock : IEventSeatLock
        {
            private bool _inside;

            public int Calls { get; private set; }

            public bool DispatchedInsideLock { get; private set; }

            public bool SavedOutsideLock { get; private set; }

            public void RecordDispatch()
            {
                DispatchedInsideLock |= _inside;
            }

            public void RecordSave()
            {
                SavedOutsideLock |= !_inside;
            }

            public async Task RunAsync(Guid eventId, Func<Task> work)
            {
                Calls++;
                _inside = true;
                try
                {
                    await work();
                }
                finally
                {
                    _inside = false;
                }
            }
        }
    }
}
