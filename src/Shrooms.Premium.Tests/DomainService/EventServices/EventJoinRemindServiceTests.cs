using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Premium.DataTransferObjects.Models.Events;
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
            _eventUtilitiesService.AnyEventsThisWeekByType(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id)).Returns(false);
            _organizationService.GetOrganizationByName("visma").Returns(GetOrganization());

            _sut.SendNotifications("visma");

            _eventUtilitiesService.Received().AnyEventsThisWeekByType(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id));
            _userEventsService.DidNotReceive().GetUsersWithAppReminders(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id));
        }

        [Test]
        public void SendNotifications_NoUsersToSendNotifications_DoesNotCreateNotification()
        {
            var eventType = new EventTypeDTO
            {
                Id = 1
            };

            _eventUtilitiesService.GetEventTypesToRemind(1).Returns(new List<EventTypeDTO> { eventType });
            _eventUtilitiesService.AnyEventsThisWeekByType(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id)).Returns(true);
            _userEventsService.GetUsersWithAppReminders(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id)).Returns(new List<string>());
            _organizationService.GetOrganizationByName("visma").Returns(GetOrganization());

            _sut.SendNotifications("visma");

            _userEventsService.Received().GetUsersWithAppReminders(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id));
            _userEventsService.Received().GetUsersWithEmailReminders(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id));
            _notificationService.DidNotReceiveWithAnyArgs().CreateForEventJoinReminder(default, default);
        }

        [Test]
        public void SendNotifications_UsersToRemind_SendsNotifications()
        {
            var eventType = new EventTypeDTO
            {
                Id = 1
            };

            var users = (IEnumerable<string>)new List<string> { "" };
            _eventUtilitiesService.GetEventTypesToRemind(1).Returns(new List<EventTypeDTO> { eventType });
            _eventUtilitiesService.AnyEventsThisWeekByType(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id)).Returns(true);
            _userEventsService.GetUsersWithAppReminders(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id)).Returns(users);
            _userEventsService.GetUsersWithEmailReminders(Arg.Is<IEnumerable<int>>(e => e.FirstOrDefault() == eventType.Id)).Returns(users);
            _organizationService.GetOrganizationByName("visma").Returns(GetOrganization());

            _sut.SendNotifications("visma");

            _notificationService.ReceivedWithAnyArgs().CreateForEventJoinReminder(users, 1);
            _eventNotificationService.ReceivedWithAnyArgs().RemindUsersToJoinEvent(Arg.Is<IEnumerable<EventTypeDTO>>(e => e.FirstOrDefault().Id == eventType.Id), users, 1);
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
