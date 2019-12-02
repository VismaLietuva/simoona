using System.Linq;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Domain.Services.Email.Event;
using Shrooms.Domain.Services.Events.Utilities;
using Shrooms.Domain.Services.Users;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Notifications;

namespace Shrooms.Domain.Services.WebHookCallbacks.Events
{
    public class EventJoinRemindService : IEventJoinRemindService
    {
        private readonly INotificationService _notificationService;
        private readonly IEventUtilitiesService _eventUtilitiesService;
        private readonly IUserEventsService _userEventsService;
        private readonly IEventNotificationService _eventNotificationService;

        public EventJoinRemindService(
            INotificationService notificationService,
            IEventUtilitiesService eventUtilitiesService,
            IUserEventsService userEventsService,
            IEventNotificationService eventNotificationService)
        {
            _notificationService = notificationService;
            _eventUtilitiesService = eventUtilitiesService;
            _userEventsService = userEventsService;
            _eventNotificationService = eventNotificationService;
        }

        public void SendNotifications(UserAndOrganizationDTO userOrg)
        {
            var typesToNotifyAbout = _eventUtilitiesService.GetEventTypesToRemind(userOrg.OrganizationId);

            foreach (var eventType in typesToNotifyAbout)
            {
                var anythingToJoin = _eventUtilitiesService.AnyEventsThisWeekByType(eventType.Id);
                if (!anythingToJoin)
                {
                    continue;
                }

                var usersToNotify = _userEventsService.GetUsersWithoutEventThisWeek(eventType.Id).ToList();

                if (usersToNotify.Any())
                {
                    _notificationService.CreateForEventJoinReminder(eventType, usersToNotify, userOrg);
                    _eventNotificationService.RemindUsersToJoinEvent(eventType, usersToNotify, userOrg.OrganizationId);
                }
            }
        }
    }
}
