using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events.Reminders;
using Shrooms.Premium.Domain.Services.Email.Event;
using Shrooms.Premium.Domain.Services.Events.Utilities;
using Shrooms.Premium.Domain.Services.Notifications;
using Shrooms.Premium.Domain.Services.Users;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Events;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    [TestFixture]
    public class EventJoinRemindServiceTests
    {
        private IEventRemindService _sut;
        private IEventUtilitiesService _eventUtilitiesService;
        private IUserEventsService _userEventsService;
        private INotificationService _notificationService;
        private IEventNotificationService _eventNotificationService;
        private IOrganizationService _organizationService;

        [SetUp]
        public void SetUp()
        {
            _notificationService = Substitute.For<INotificationService>();
            _eventUtilitiesService = Substitute.For<IEventUtilitiesService>();
            _userEventsService = Substitute.For<IUserEventsService>();
            _eventNotificationService = Substitute.For<IEventNotificationService>();
            _organizationService = Substitute.For<IOrganizationService>();

            _sut = new EventRemindService(
                _notificationService,
                _eventUtilitiesService,
                _userEventsService,
                _eventNotificationService,
                _organizationService);
        }

        [Test]
        public async Task SendNotifications_NoneTypesToRemind_DoesNothing()
        {
            _eventUtilitiesService.GetEventTypesToRemindAsync(1).Returns(new List<EventTypeDto>());
            _organizationService.GetOrganizationByNameAsync("visma").Returns(GetOrganization());

            await _sut.SendNotificationsAsync("visma");

            await _eventUtilitiesService.Received().GetEventTypesToRemindAsync(1);
            await _eventUtilitiesService.DidNotReceiveWithAnyArgs().AnyEventsThisWeekByTypeAsync(default);
        }

        [Test]
        public async Task SendNotifications_NothingToJoin_DoesNotRemind()
        {
            var eventType = new EventTypeDto
            {
                Id = 1
            };

            _eventUtilitiesService.GetEventTypesToRemindAsync(1).Returns(new List<EventTypeDto> { eventType });
            _eventUtilitiesService.AnyEventsThisWeekByTypeAsync(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id)).Returns(false);
            _organizationService.GetOrganizationByNameAsync("visma").Returns(GetOrganization());

            await _sut.SendNotificationsAsync("visma");

            await _eventUtilitiesService.Received().AnyEventsThisWeekByTypeAsync(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id));
            await _userEventsService.DidNotReceive().GetUsersWithAppRemindersAsync(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id));
        }

        [Test]
        public async Task SendNotifications_NoUsersToSendNotifications_DoesNotCreateNotification()
        {
            var eventType = new EventTypeDto
            {
                Id = 1
            };

            _eventUtilitiesService.GetEventTypesToRemindAsync(1).Returns(new List<EventTypeDto> { eventType });
            _eventUtilitiesService.AnyEventsThisWeekByTypeAsync(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id)).Returns(true);
            _userEventsService.GetUsersWithAppRemindersAsync(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id)).Returns(new List<string>());
            _organizationService.GetOrganizationByNameAsync("visma").Returns(GetOrganization());

            await _sut.SendNotificationsAsync("visma");

            await _userEventsService.Received().GetUsersWithAppRemindersAsync(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id));
            await _userEventsService.Received().GetUsersWithEmailRemindersAsync(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id));
            await _notificationService.DidNotReceiveWithAnyArgs().CreateForEventJoinReminderAsync(default, default);
        }

        [Test]
        public async Task SendNotifications_UsersToRemind_SendsNotifications()
        {
            var eventType = new EventTypeDto
            {
                Id = 1
            };

            var users = new List<string> { "" };
            _eventUtilitiesService.GetEventTypesToRemindAsync(1).Returns(new List<EventTypeDto> { eventType });
            _eventUtilitiesService.AnyEventsThisWeekByTypeAsync(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id)).Returns(true);
            _userEventsService.GetUsersWithAppRemindersAsync(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id)).Returns(users);
            _userEventsService.GetUsersWithEmailRemindersAsync(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id)).Returns(users);
            _organizationService.GetOrganizationByNameAsync("visma").Returns(GetOrganization());

            await _sut.SendNotificationsAsync("visma");

            await _notificationService.ReceivedWithAnyArgs().CreateForEventJoinReminderAsync(users, 1);
            await _eventNotificationService.ReceivedWithAnyArgs().RemindUsersToJoinEventAsync(Arg.Is<IEnumerable<EventTypeDto>>(e => e.FirstOrDefault().Id == eventType.Id), users, 1);
        }

        [Test]
        public async Task SendJoinedNotificationsAsync_ValidValues_SendsEmails()
        {
            // Arrange
            const string organizationName = "org";
            var organization = new Organization { Id = 1, ShortName = organizationName };
            _organizationService.GetOrganizationByNameAsync(Arg.Is(organizationName))
                .Returns(organization);

            var @event = new Event
            {
                RegistrationDeadline = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                Name = "test",
                Id = Guid.NewGuid(),
                EventParticipants = new List<EventParticipant>
                {
                    new EventParticipant
                    {
                        AttendStatus = (int)AttendingStatus.Attending,
                        ApplicationUser = new ApplicationUser
                        {
                            Email = "testEmail1",
                            TimeZone = "testTimeZone1"
                        }
                    },
                    new EventParticipant
                    {
                        AttendStatus = (int)AttendingStatus.AttendingVirtually,
                        ApplicationUser = new ApplicationUser
                        {
                            Email = "testEmail2",
                            TimeZone = "testTimeZone2"
                        }
                    },
                    new EventParticipant
                    {
                        AttendStatus = (int)AttendingStatus.MaybeAttending,
                        ApplicationUser = new ApplicationUser
                        {
                            Email = "testEmail3",
                            TimeZone = "testTimeZone3"
                        }
                    }
                }
            };
            var reminders = new List<EventReminder>
            {
                new EventReminder
                {
                    Event = @event,
                    EventId = @event.Id,
                    RemindBeforeInDays = 10,
                    IsReminded = false,
                    Type = EventReminderType.Start
                },
                new EventReminder
                {
                    Event = @event,
                    EventId = @event.Id,
                    RemindBeforeInDays = 10,
                    IsReminded = false,
                    Type = EventReminderType.Deadline
                }
            };
            _userEventsService.GetReadyNotCompletedRemindersAsync(Arg.Is(organization))
                .Returns(reminders);

            // Act
            await _sut.SendJoinedNotificationsAsync(organizationName);

            // Assert
            await _organizationService.Received(1)
                .GetOrganizationByNameAsync(Arg.Is(organizationName));
            await _eventNotificationService.Received(1)
                .RemindUsersAboutStartDateOfJoinedEventsAsync(
                    Arg.Is<IEnumerable<EventReminderStartEmailDto>>(entries => entries.Count() == 1),
                    Arg.Is(organization));
            await _eventNotificationService.Received(1)
                .RemindUsersAboutDeadlineDateOfJoinedEventsAsync(
                    Arg.Is<IEnumerable<EventReminderDeadlineEmailDto>>(entries => entries.Count() == 1),
                    Arg.Is(organization));
            await _userEventsService.Received(1).SetRemindersAsCompleteAsync(Arg.Is(reminders));
        }

        private static Organization GetOrganization()
        {
            return new Organization
            {
                Id = 1
            };
        }
    }
}
