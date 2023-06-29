using System;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events.Reminders;
using Shrooms.Premium.Domain.Services.Email.Event;
using Shrooms.Premium.Domain.Services.Events.Utilities;
using Shrooms.Premium.Domain.Services.Notifications;
using Shrooms.Premium.Domain.Services.Users;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks.Events
{
    public class EventRemindService : IEventRemindService
    {
        private readonly INotificationService _notificationService;
        private readonly IEventUtilitiesService _eventUtilitiesService;
        private readonly IUserEventsService _userEventsService;
        private readonly IEventNotificationService _eventNotificationService;
        private readonly IOrganizationService _organizationService;

        public EventRemindService(
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

        public async Task SendJoinedNotificationsAsync(string organizationName)
        {
            var organization = await GetOrganizationAsync(organizationName);
            var reminders = await _userEventsService.GetReadyNotCompletedRemindersAsync(organization);

            var startEmailDtos = reminders.Where(reminder => reminder.Type == EventReminderType.Start)
                .Select(MapRemindStartEvent())
                .ToList();
            await _eventNotificationService.RemindUsersAboutStartDateOfJoinedEventsAsync(startEmailDtos, organization);
            var deadlineEmailDtos = reminders.Where(reminder => reminder.Type == EventReminderType.Deadline)
                .Select(MapRemindDeadlineEvent())
                .ToList();
            await _eventNotificationService.RemindUsersAboutDeadlineDateOfJoinedEventsAsync(deadlineEmailDtos, organization);

            await _userEventsService.SetRemindersAsCompleteAsync(reminders);
        }

        public async Task SendNotificationsAsync(string organizationName)
        {
            var organization = await GetOrganizationAsync(organizationName);

            var typesToNotifyAbout = (await _eventUtilitiesService.GetEventTypesToRemindAsync(organization.Id)).ToList();
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
                await _notificationService.CreateForEventJoinReminderAsync(usersToNotifyInApp, organization.Id);
            }

            if (usersToNotifyEmail.Any())
            {
                await _eventNotificationService.RemindUsersToJoinEventAsync(typesToNotifyAbout, usersToNotifyEmail, organization.Id);
            }
        }

        private async Task<Organization> GetOrganizationAsync(string organizationName)
        {
            return await _organizationService.GetOrganizationByNameAsync(organizationName);
        }

        private Func<EventReminder, EventReminderStartEmailDto> MapRemindStartEvent()
        {
            return reminder => new EventReminderStartEmailDto
            {
                StartDate = reminder.Event.StartDate,
                EventName = reminder.Event.Name,
                EventId = reminder.Event.Id,
                Receivers = reminder.Event.EventParticipants
                    .Where(FilterByEventWeeklyNotificationSetting())
                    .Where(FilterAttendingParticipants())
                    .Select(participant => new EventReminderEmailReceiverDto
                    {
                        Email = participant.ApplicationUser.Email,
                        TimeZoneKey = participant.ApplicationUser.TimeZone
                    })
                    .ToList()
            };
        }

        private Func<EventReminder, EventReminderDeadlineEmailDto> MapRemindDeadlineEvent()
        {
            return reminder => new EventReminderDeadlineEmailDto
            {
                DeadlineDate = reminder.Event.RegistrationDeadline,
                StartDate = reminder.Event.StartDate,
                EventName = reminder.Event.Name,
                EventId = reminder.Event.Id,
                Receivers = reminder.Event.EventParticipants
                    .Where(FilterByEventWeeklyNotificationSetting())
                    .Where(FilterAttendingParticipants())
                    .Select(participant => new EventReminderEmailReceiverDto
                    {
                        Email = participant.ApplicationUser.Email,
                        TimeZoneKey = participant.ApplicationUser.TimeZone
                    })
                    .ToList()
            };
        }

        private static Func<EventParticipant, bool> FilterByEventWeeklyNotificationSetting()
        {
            return participant =>
                participant.ApplicationUser.NotificationsSettings == null ||
                participant.ApplicationUser.NotificationsSettings.EventWeeklyReminderEmailNotifications;
        }

        private static Func<EventParticipant, bool> FilterAttendingParticipants()
        {
            return participant => participant.AttendStatus == (int)AttendingStatus.Attending || participant.AttendStatus == (int)AttendingStatus.AttendingVirtually;
        }
    }
}
