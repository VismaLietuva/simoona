using System.Linq;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Event;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.Utilities;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Notifications;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Users;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.Events
{
    public class EventJoinRemindService : IEventJoinRemindService
    {
        private readonly INotificationService _notificationService;
        private readonly IEventUtilitiesService _eventUtilitiesService;
        private readonly IUserEventsService _userEventsService;
        private readonly IEventNotificationService _eventNotificationService;
        private readonly IOrganizationService _organizationService;

        public EventJoinRemindService(
            INotificationService notificationService,
            IEventUtilitiesService eventUtilitiesService,
            IUserEventsService userEventsService,
            IEventNotificationService eventNotificationService,
            IOrganizationService organizationService)
        {
            _notificationService = notificationService;
            _eventUtilitiesService = eventUtilitiesService;
            _userEventsService = userEventsService;
            _eventNotificationService = eventNotificationService;
            _organizationService = organizationService;
        }

        public void SendNotifications(string orgName)
        {
            var userOrg = _organizationService.GetOrganizationByName(orgName);

            var typesToNotifyAbout = _eventUtilitiesService.GetEventTypesToRemind(userOrg.Id);

            foreach (var eventType in typesToNotifyAbout)
            {
                var anythingToJoin = _eventUtilitiesService.AnyEventsThisWeekByType(eventType.Id);
                if (!anythingToJoin)
                {
                    continue;
                }

                var usersToNotifyInApp = _userEventsService.GetUsersWithAppReminders(eventType.Id).ToList();
                var usersToNotifyEmail = _userEventsService.GetUsersWithEmailReminders(eventType.Id).ToList();

                if (usersToNotifyInApp.Any())
                {
                    _notificationService.CreateForEventJoinReminder(eventType, usersToNotifyInApp, userOrg.Id);
                }

                if (usersToNotifyEmail.Any())
                {
                    _eventNotificationService.RemindUsersToJoinEvent(eventType, usersToNotifyEmail, userOrg.Id);
                }
            }
        }
    }
}
