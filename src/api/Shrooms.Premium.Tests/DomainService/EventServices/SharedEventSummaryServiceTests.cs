using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Premium.Constants;
using Shrooms.Premium.Domain.Services.Events;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    [TestFixture]
    public class SharedEventSummaryServiceTests
    {
        private const string ViewerId = "viewer";
        private const int OrganizationId = 2;

        private DbSet<Event> _eventsDbSet;
        private IPermissionService _permissionService;
        private SharedEventSummaryService _service;

        [SetUp]
        public void SetUp()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            _eventsDbSet = uow.MockDbSetForAsync<Event>();
            _permissionService = Substitute.For<IPermissionService>();
            MockPermissions(canSeeEvents: true, canSeeAll: true);

            _service = new SharedEventSummaryService(uow, _permissionService);
        }

        [Test]
        public async Task Should_Return_The_Card_Fields_For_Each_Event()
        {
            var @event = NewEvent(maxParticipants: 2);
            @event.EventParticipants = new List<EventParticipant>
            {
                Participant(1, "a", AttendingStatus.Attending),
                Participant(2, ViewerId, AttendingStatus.Attending),
                Participant(3, "b", AttendingStatus.AttendingVirtually),
                Participant(4, "c", AttendingStatus.MaybeAttending),
                Participant(5, "d", AttendingStatus.NotAttending)
            };
            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event });

            var summary = (await _service.GetSummariesAsync(new[] { @event.Id }, Viewer()))[@event.Id];

            Assert.That(summary.Name, Is.EqualTo("Pizza Friday"));
            Assert.That(summary.ImageName, Is.EqualTo("pizza.png"));
            Assert.That(summary.Location, Is.EqualTo("Kitchen"));
            Assert.That(summary.StartDate, Is.EqualTo(@event.StartDate));
            Assert.That(summary.EndDate, Is.EqualTo(@event.EndDate));
            Assert.That(summary.RegistrationDeadlineDate, Is.EqualTo(@event.RegistrationDeadline));
            Assert.That(summary.GoingCount, Is.EqualTo(3));
            Assert.That(summary.IsFull, Is.True, "two people going in person fill two seats");
            Assert.That(summary.IsAttending, Is.True);
            Assert.That(summary.HasResponse, Is.True);
            Assert.That(summary.Attendees.Select(a => a.UserId), Is.EqualTo(new[] { "a", "b" }));
            Assert.That(summary.Attendees.First().FullName, Is.EqualTo("First-a Last-a"));
            Assert.That(summary.Attendees.First().ImageName, Is.EqualTo("a.png"));
        }

        [Test]
        public async Task Should_Not_Count_Virtual_Attendees_Towards_Full()
        {
            var @event = NewEvent(maxParticipants: 1);
            @event.EventParticipants = new List<EventParticipant> { Participant(1, "a", AttendingStatus.AttendingVirtually) };
            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event });

            var summary = (await _service.GetSummariesAsync(new[] { @event.Id }, Viewer()))[@event.Id];

            Assert.That(summary.IsFull, Is.False);
        }

        [Test]
        public async Task Should_Return_At_Most_Three_Attendees_In_Participant_Order()
        {
            var @event = NewEvent();
            @event.EventParticipants = new List<EventParticipant>
            {
                Participant(9, "late", AttendingStatus.Attending),
                Participant(1, "first", AttendingStatus.Attending),
                Participant(2, "second", AttendingStatus.Attending),
                Participant(3, "third", AttendingStatus.Attending)
            };
            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event });

            var summary = (await _service.GetSummariesAsync(new[] { @event.Id }, Viewer()))[@event.Id];

            Assert.That(summary.Attendees.Select(a => a.UserId), Is.EqualTo(new[] { "first", "second", "third" }));
            Assert.That(summary.GoingCount, Is.EqualTo(4));
        }

        [Test]
        public async Task Should_Show_A_Deleted_Attendee_Without_A_User_Id()
        {
            var @event = NewEvent();
            var deleted = Participant(1, "gone", AttendingStatus.Attending);
            deleted.ApplicationUser = null;
            @event.EventParticipants = new List<EventParticipant> { deleted };
            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event });

            var summary = (await _service.GetSummariesAsync(new[] { @event.Id }, Viewer()))[@event.Id];

            Assert.That(summary.Attendees.Single().UserId, Is.Empty);
        }

        [Test]
        public async Task Should_Hide_Attendees_From_A_Viewer_Who_Cannot_See_Participants()
        {
            MockPermissions(canSeeEvents: true, canSeeAll: false);
            var @event = NewEvent();
            @event.EventParticipants = new List<EventParticipant>
            {
                Participant(1, "a", AttendingStatus.Attending),
                Participant(2, ViewerId, AttendingStatus.Attending)
            };
            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event });

            var summary = (await _service.GetSummariesAsync(new[] { @event.Id }, Viewer()))[@event.Id];

            Assert.That(summary.Attendees, Is.Empty);
            Assert.That(summary.GoingCount, Is.EqualTo(2), "counts cover everyone, like Events/Details");
            Assert.That(summary.IsAttending, Is.True);
        }

        [TestCase(null, false, false)]
        [TestCase(AttendingStatus.Idle, false, false)]
        [TestCase(AttendingStatus.NotAttending, false, true)]
        [TestCase(AttendingStatus.MaybeAttending, false, true)]
        [TestCase(AttendingStatus.Attending, true, true)]
        [TestCase(AttendingStatus.AttendingVirtually, true, true)]
        public async Task Should_Report_The_Viewers_Own_Answer(AttendingStatus? status, bool isAttending, bool hasResponse)
        {
            var @event = NewEvent();
            @event.EventParticipants = status.HasValue
                ? new List<EventParticipant> { Participant(1, ViewerId, status.Value) }
                : new List<EventParticipant>();
            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event });

            var summary = (await _service.GetSummariesAsync(new[] { @event.Id }, Viewer()))[@event.Id];

            Assert.That(summary.IsAttending, Is.EqualTo(isAttending));
            Assert.That(summary.HasResponse, Is.EqualTo(hasResponse));
        }

        [TestCase(0, false, false, false)]
        [TestCase(0, true, false, false)]
        [TestCase(1, true, false, true)]
        [TestCase(1, false, false, false)]
        [TestCase(0, false, true, true)]
        public async Task Should_Say_When_Joining_Needs_The_Sign_Up_Wizard(int maxChoices, bool hasFlatOption, bool hasQuestion, bool expected)
        {
            var @event = NewEvent();
            @event.MaxChoices = maxChoices;
            @event.EventOptions = hasFlatOption
                ? new List<EventOption> { new() { Id = 1, Option = "Pizza" } }
                : new List<EventOption>();
            @event.EventQuestions = hasQuestion
                ? new List<EventQuestion> { new() { Id = 1, Title = "Dish?" } }
                : new List<EventQuestion>();
            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event });

            var summary = (await _service.GetSummariesAsync(new[] { @event.Id }, Viewer()))[@event.Id];

            Assert.That(summary.NeedsSignUpSteps, Is.EqualTo(expected));
        }

        [Test]
        public async Task Should_Not_Count_Question_Options_As_Flat_Options()
        {
            var @event = NewEvent();
            @event.MaxChoices = 1;
            @event.EventOptions = new List<EventOption> { new() { Id = 1, Option = "Pasta", QuestionId = 5 } };
            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event });

            var summary = (await _service.GetSummariesAsync(new[] { @event.Id }, Viewer()))[@event.Id];

            Assert.That(summary.NeedsSignUpSteps, Is.False);
        }

        [Test]
        public async Task Should_Leave_Out_Events_Of_Another_Organization_And_Unknown_Ids()
        {
            var ours = NewEvent();
            var theirs = NewEvent();
            theirs.OrganizationId = 3;
            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { ours, theirs });

            var summaries = await _service.GetSummariesAsync(new[] { ours.Id, theirs.Id, Guid.NewGuid() }, Viewer());

            Assert.That(summaries.Keys, Is.EquivalentTo(new[] { ours.Id }));
        }

        [Test]
        public async Task Should_Return_Nothing_To_A_Viewer_Without_The_Event_Permission()
        {
            MockPermissions(canSeeEvents: false, canSeeAll: true);
            var @event = NewEvent();
            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event });

            var summaries = await _service.GetSummariesAsync(new[] { @event.Id }, Viewer());

            Assert.That(summaries, Is.Empty);
        }

        [Test]
        public async Task Should_Not_Query_Without_Event_Ids()
        {
            var summaries = await _service.GetSummariesAsync(Array.Empty<Guid>(), Viewer());

            Assert.That(summaries, Is.Empty);
            await _permissionService.DidNotReceiveWithAnyArgs().UserHasPermissionAsync(default, default, default);
        }

        [Test]
        public void Should_Stop_The_Query_When_The_Token_Is_Cancelled()
        {
            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { NewEvent() });

            Assert.That(
                async () => await _service.GetSummariesAsync(new[] { Guid.NewGuid() }, Viewer(), new CancellationToken(true)),
                Throws.InstanceOf<OperationCanceledException>());
        }

        private void MockPermissions(bool canSeeEvents, bool canSeeAll)
        {
            _permissionService.UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), BasicPermissions.Event, Arg.Any<CancellationToken>()).Returns(canSeeEvents);
            _permissionService.UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), BasicPermissions.EventUsers, Arg.Any<CancellationToken>()).Returns(canSeeAll);
        }

        private static UserAndOrganizationDto Viewer()
        {
            return new UserAndOrganizationDto { UserId = ViewerId, OrganizationId = OrganizationId };
        }

        private static Event NewEvent(int maxParticipants = 10)
        {
            return new Event
            {
                Id = Guid.NewGuid(),
                OrganizationId = OrganizationId,
                Name = "Pizza Friday",
                ImageName = "pizza.png",
                Place = "Kitchen",
                StartDate = new DateTime(2026, 12, 4, 12, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 12, 4, 14, 0, 0, DateTimeKind.Utc),
                RegistrationDeadline = new DateTime(2026, 12, 3, 12, 0, 0, DateTimeKind.Utc),
                MaxParticipants = maxParticipants,
                EventParticipants = new List<EventParticipant>(),
                EventOptions = new List<EventOption>(),
                EventQuestions = new List<EventQuestion>()
            };
        }

        private static EventParticipant Participant(int id, string userId, AttendingStatus status)
        {
            return new EventParticipant
            {
                Id = id,
                ApplicationUserId = userId,
                AttendStatus = (int)status,
                ApplicationUser = new ApplicationUser
                {
                    Id = userId,
                    FirstName = $"First-{userId}",
                    LastName = $"Last-{userId}",
                    PictureId = $"{userId}.png"
                }
            };
        }
    }
}
