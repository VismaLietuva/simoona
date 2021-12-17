using System.Linq;
using System.Threading.Tasks;
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

        public EventJoinRemindService(INotificationService notificationService,
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

        public async Task SendNotificationsAsync(string orgName)
        {
            var userOrg = await _organizationService.GetOrganizationByNameAsync(orgName);

            var typesToNotifyAbout = (await _eventUtilitiesService.GetEventTypesToRemindAsync(userOrg.Id)).ToList();
            var typeIdsToNotifyAbout = typesToNotifyAbout.Select(e => e.Id).ToList();

            if (!typesToNotifyAbout.Any())
            {
                return;
            }

            var anythingToJoin = await _eventUtilitiesService.AnyEventsThisWeekByTypeAsync(typeIdsToNotifyAbout);
            if (!anythingToJoin)
            {
                return;
            }

            var usersToNotifyInApp = (await _userEventsService.GetUsersWithAppRemindersAsync(typeIdsToNotifyAbout)).ToList();
            var usersToNotifyEmail = (await _userEventsService.GetUsersWithEmailRemindersAsync(typeIdsToNotifyAbout)).ToList();

            if (usersToNotifyInApp.Any())
            {
                await _notificationService.CreateForEventJoinReminderAsync(usersToNotifyInApp, userOrg.Id);
            }

            if (usersToNotifyEmail.Any())
            {
                await _eventNotificationService.RemindUsersToJoinEventAsync(typesToNotifyAbout, usersToNotifyEmail, userOrg.Id);
            }
        }
    }
}
