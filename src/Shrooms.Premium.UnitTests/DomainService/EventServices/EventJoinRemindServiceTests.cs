using NSubstitute;
using NUnit.Framework;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.Domain.Services.Email.Event;
using Shrooms.Domain.Services.Events.Utilities;
using Shrooms.Domain.Services.Users;
using Shrooms.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Notifications;
using System.Collections.Generic;

namespace Shrooms.Premium.UnitTests.DomainService.EventServices
{
    [TestFixture]
    public class EventJoinRemindServiceTests
    {
        private IEventJoinRemindService _sut;
        private IEventUtilitiesService _eventUtilitiesService;
        private IUserEventsService _userEventsService;
        private INotificationService _notificationService;
        private IEventNotificationService _eventNotificationService;

        [SetUp]
        public void SetUp()
        {
            _notificationService = Substitute.For<INotificationService>();
            _eventUtilitiesService = Substitute.For<IEventUtilitiesService>();
            _userEventsService = Substitute.For<IUserEventsService>();
            _eventNotificationService = Substitute.For<IEventNotificationService>();

            _sut = new EventJoinRemindService(_notificationService, _eventUtilitiesService, _userEventsService, _eventNotificationService);
        }

        [Test]
        public void SendNotifications_NoneTypesToRemind_DoesNothing()
        {
            var userOrg = GetUserOrg();
            _eventUtilitiesService.GetEventTypesToRemind(userOrg.OrganizationId).Returns(new List<EventTypeDTO>());

            _sut.SendNotifications(userOrg);

            _eventUtilitiesService.Received().GetEventTypesToRemind(userOrg.OrganizationId);
            _eventUtilitiesService.DidNotReceiveWithAnyArgs().AnyEventsThisWeekByType(default);
        }

        [Test]
        public void SendNotifications_NothingToJoin_DoesNotRemind()
        {
            var userOrg = GetUserOrg();
            var eventType = new EventTypeDTO
            {
                Id = 1
            };
            _eventUtilitiesService.GetEventTypesToRemind(userOrg.OrganizationId).Returns(new List<EventTypeDTO> { eventType });
            _eventUtilitiesService.AnyEventsThisWeekByType(eventType.Id).Returns(false);

            _sut.SendNotifications(userOrg);

            _eventUtilitiesService.Received().AnyEventsThisWeekByType(eventType.Id);
            _userEventsService.DidNotReceive().GetUsersWithAppReminders(eventType.Id);
        }

        [Test]
        public void SendNotifications_NoUsersToSendNotifications_DoesNotCreateNotification()
        {
            var userOrg = GetUserOrg();
            var eventType = new EventTypeDTO
            {
                Id = 1
            };
            _eventUtilitiesService.GetEventTypesToRemind(userOrg.OrganizationId).Returns(new List<EventTypeDTO> { eventType });
            _eventUtilitiesService.AnyEventsThisWeekByType(eventType.Id).Returns(true);
            _userEventsService.GetUsersWithAppReminders(eventType.Id).Returns(new List<string>());

            _sut.SendNotifications(userOrg);

            _userEventsService.Received().GetUsersWithAppReminders(eventType.Id);
            _userEventsService.Received().GetUsersWithEmailReminders(eventType.Id);
            _notificationService.DidNotReceiveWithAnyArgs().CreateForEventJoinReminder(default, default, default);
        }

        [Test]
        public void SendNotifications_UsersToRemind_SendsNotifications()
        {
            var userOrg = GetUserOrg();
            var eventType = new EventTypeDTO
            {
                Id = 1
            };
            var users = new List<string> { "" };
            _eventUtilitiesService.GetEventTypesToRemind(userOrg.OrganizationId).Returns(new List<EventTypeDTO> { eventType });
            _eventUtilitiesService.AnyEventsThisWeekByType(eventType.Id).Returns(true);
            _userEventsService.GetUsersWithAppReminders(eventType.Id).Returns(users);
            _userEventsService.GetUsersWithEmailReminders(eventType.Id).Returns(users);

            _sut.SendNotifications(userOrg);

            _notificationService.CreateForEventJoinReminder(eventType, users, userOrg);
            _eventNotificationService.RemindUsersToJoinEvent(eventType, users, userOrg.OrganizationId);
        }

        private static UserAndOrganizationDTO GetUserOrg()
        {
            return new UserAndOrganizationDTO
            {
                OrganizationId = 1,
                UserId = "1"
            };
        }
    }
}
