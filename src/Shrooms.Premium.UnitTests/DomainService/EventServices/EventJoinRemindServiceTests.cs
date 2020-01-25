using NSubstitute;
using NUnit.Framework;
using Shrooms.Domain.Services.Organizations;
using Shrooms.EntityModels.Models;
using System.Collections.Generic;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Event;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.Utilities;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Notifications;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Users;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.Events;

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
        private IOrganizationService _organizationService;

        [SetUp]
        public void SetUp()
        {
            _notificationService = Substitute.For<INotificationService>();
            _eventUtilitiesService = Substitute.For<IEventUtilitiesService>();
            _userEventsService = Substitute.For<IUserEventsService>();
            _eventNotificationService = Substitute.For<IEventNotificationService>();
            _organizationService = Substitute.For<IOrganizationService>();

            _sut = new EventJoinRemindService(_notificationService, _eventUtilitiesService, _userEventsService, _eventNotificationService, _organizationService);
        }

        [Test]
        public void SendNotifications_NoneTypesToRemind_DoesNothing()
        {
            _eventUtilitiesService.GetEventTypesToRemind(1).Returns(new List<EventTypeDTO>());
            _organizationService.GetOrganizationByName("visma").Returns(GetOrganization());

            _sut.SendNotifications("visma");

            _eventUtilitiesService.Received().GetEventTypesToRemind(1);
            _eventUtilitiesService.DidNotReceiveWithAnyArgs().AnyEventsThisWeekByType(default);
        }

        [Test]
        public void SendNotifications_NothingToJoin_DoesNotRemind()
        {
            var eventType = new EventTypeDTO
            {
                Id = 1
            };
            _eventUtilitiesService.GetEventTypesToRemind(1).Returns(new List<EventTypeDTO> { eventType });
            _eventUtilitiesService.AnyEventsThisWeekByType(eventType.Id).Returns(false);
            _organizationService.GetOrganizationByName("visma").Returns(GetOrganization());

            _sut.SendNotifications("visma");

            _eventUtilitiesService.Received().AnyEventsThisWeekByType(eventType.Id);
            _userEventsService.DidNotReceive().GetUsersWithAppReminders(eventType.Id);
        }

        [Test]
        public void SendNotifications_NoUsersToSendNotifications_DoesNotCreateNotification()
        {
            var eventType = new EventTypeDTO
            {
                Id = 1
            };
            _eventUtilitiesService.GetEventTypesToRemind(1).Returns(new List<EventTypeDTO> { eventType });
            _eventUtilitiesService.AnyEventsThisWeekByType(eventType.Id).Returns(true);
            _userEventsService.GetUsersWithAppReminders(eventType.Id).Returns(new List<string>());
            _organizationService.GetOrganizationByName("visma").Returns(GetOrganization());

            _sut.SendNotifications("visma");

            _userEventsService.Received().GetUsersWithAppReminders(eventType.Id);
            _userEventsService.Received().GetUsersWithEmailReminders(eventType.Id);
            _notificationService.DidNotReceiveWithAnyArgs().CreateForEventJoinReminder(default, default, default);
        }

        [Test]
        public void SendNotifications_UsersToRemind_SendsNotifications()
        {
            var eventType = new EventTypeDTO
            {
                Id = 1
            };
            var users = (IEnumerable<string>) new List<string> { "" };
            _eventUtilitiesService.GetEventTypesToRemind(1).Returns(new List<EventTypeDTO> { eventType });
            _eventUtilitiesService.AnyEventsThisWeekByType(eventType.Id).Returns(true);
            _userEventsService.GetUsersWithAppReminders(eventType.Id).Returns(users);
            _userEventsService.GetUsersWithEmailReminders(eventType.Id).Returns(users);
            _organizationService.GetOrganizationByName("visma").Returns(GetOrganization());

            _sut.SendNotifications("visma");

            _notificationService.ReceivedWithAnyArgs().CreateForEventJoinReminder(eventType, users, 1);
            _eventNotificationService.ReceivedWithAnyArgs().RemindUsersToJoinEvent(eventType, users, 1);
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
