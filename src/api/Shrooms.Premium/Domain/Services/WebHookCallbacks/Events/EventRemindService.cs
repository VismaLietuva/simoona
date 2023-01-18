using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Services.Organizations;
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
        private readonly ISystemClock _systemClock;

        public EventRemindService(
            INotificationService notificationService,
            IEventUtilitiesService eventUtilitiesService,
            IUserEventsService userEventsService,
            IEventNotificationService eventNotificationService,
            IOrganizationService organizationService,
            ISystemClock systemClock)
        {
            _notificationService = notificationService;
            _eventUtilitiesService = eventUtilitiesService;
            _userEventsService = userEventsService;
            _eventNotificationService = eventNotificationService;
            _organizationService = organizationService;
            _systemClock = systemClock;
        }

        public async Task SendJoinedNotificationsAsync(string organizationName)
        {
            var organization = await GetOrganizationAsync(organizationName);
            var reminders = await _userEventsService.GetNotCompletedRemindersAsync(organization);
            await _eventNotificationService.RemindAllUsersAboutJoinedEventsAsync(MapRemindersToRemindJoinedEvent(reminders), organization);
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

        private RemindJoinedEventEmailDto MapRemindersToRemindJoinedEvent(IEnumerable<EventReminder> reminders)
        {
            var remindDeadlineEvents = reminders.Where(FilterByValidReminder(reminder => reminder.Event.RegistrationDeadline, EventRemindType.Deadline))
                .Select(MapRemindDeadlineEvent())
                .ToList();
            var remindStartEvents = reminders.Where(FilterByValidReminder(reminder => reminder.Event.StartDate, EventRemindType.Start))
                .Select(MapRemindStartEvent())
                .ToList();

            return new RemindJoinedEventEmailDto
            {
                RemindDeadlineEvents = remindDeadlineEvents,
                RemindStartEvents = remindStartEvents
            };
        }

        private Func<EventReminder, bool> FilterByValidReminder(Func<EventReminder, DateTime> dateFunc, EventRemindType type)
        {
            return reminder => reminder.Type == type && dateFunc(reminder).AddDays(-reminder.RemindBeforeInDays) >= _systemClock.UtcNow;
        }

        private Func<EventReminder, RemindEventStartEmailDto> MapRemindStartEvent()
        {
            return reminder => new RemindEventStartEmailDto
            {
                StartDate = reminder.Event.StartDate,
                EventName = reminder.Event.Name,
                EventId = reminder.Event.Id,
                Receivers = reminder.Event.EventParticipants.Select(participant => new RemindReceiverDto 
                {
                    Email = participant.ApplicationUser.Email,
                    TimeZone = participant.ApplicationUser.TimeZone,
                }).ToList()
            };
        }

        private Func<EventReminder, RemindEventDeadlineEmailDto> MapRemindDeadlineEvent()
        {
            return reminder => new RemindEventDeadlineEmailDto
            {
                DeadlineDate = reminder.Event.RegistrationDeadline,
                StartDate = reminder.Event.StartDate,
                EventName = reminder.Event.Name,
                EventId = reminder.Event.Id,
                Receivers = reminder.Event.EventParticipants.Select(participant => new RemindReceiverDto
                {
                    Email = participant.ApplicationUser.Email,
                    TimeZone = participant.ApplicationUser.TimeZone
                }).ToList()
            };
        }
    }
}
