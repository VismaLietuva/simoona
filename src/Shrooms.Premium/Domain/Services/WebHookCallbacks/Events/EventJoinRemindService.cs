using System.Linq;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Premium.Domain.Services.Email.Event;
using Shrooms.Premium.Domain.Services.Events.Utilities;
using Shrooms.Premium.Domain.Services.Notifications;
using Shrooms.Premium.Domain.Services.Users;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks.Events
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

            var typesToNotifyAbout = _eventUtilitiesService.GetEventTypesToRemind(userOrg.Id).ToList();
            var typeIdsToNotifyAbout = typesToNotifyAbout.Select(e => e.Id).ToList();

            if (!typesToNotifyAbout.Any())
            {
                return;
            }

            var anythingToJoin = _eventUtilitiesService.AnyEventsThisWeekByType(typeIdsToNotifyAbout);
            if (!anythingToJoin)
            {
                return;
            }

            var usersToNotifyInApp = _userEventsService.GetUsersWithAppReminders(typeIdsToNotifyAbout).ToList();
            var usersToNotifyEmail = _userEventsService.GetUsersWithEmailReminders(typeIdsToNotifyAbout).ToList();

            if (usersToNotifyInApp.Any())
            {
                _notificationService.CreateForEventJoinReminder(usersToNotifyInApp, userOrg.Id);
            }

            if (usersToNotifyEmail.Any())
            {
                _eventNotificationService.RemindUsersToJoinEventAsync(typesToNotifyAbout, usersToNotifyEmail, userOrg.Id);
            }
        }
    }
}