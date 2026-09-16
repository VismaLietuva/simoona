using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;
using MailAttachment = System.Net.Mail.Attachment;

namespace Shrooms.Premium.Domain.Services.Events.Calendar
{
    public class EventCalendarService : IEventCalendarService
    {
        private readonly DbSet<ApplicationUser> _usersDbSet;

        private readonly DbSet<Event> _eventsDbSet;
        private readonly DbSet<Organization> _organizationsDbSet;
        private readonly DbSet<EventParticipant> _eventParticipantsDbSet;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _appSettings;
        private IEventValidationService _eventValidationService;

        public EventCalendarService(IUnitOfWork2 uow, IMailingService mailingService, IApplicationSettings appSettings, IEventValidationService eventValidationService)
        {
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _eventsDbSet = uow.GetDbSet<Event>();
            _organizationsDbSet = uow.GetDbSet<Organization>();
            _eventParticipantsDbSet = uow.GetDbSet<EventParticipant>();
            _mailingService = mailingService;
            _appSettings = appSettings;
            _eventValidationService = eventValidationService;
        }

        public async Task SendInvitationAsync(EventJoinValidationDto @event, IEnumerable<string> userIds, int orgId)
        {
            var emails = await _usersDbSet
                .Where(u => userIds.Contains(u.Id))
                .Select(u => u.Email)
                .ToListAsync();

            var calendarEvent = MapToCalendarEvent(@event);
            AddChoices(calendarEvent, @event.Name, @event.SelectedOptions);
            await AddEventLinkToDescriptionAsync(calendarEvent, @event.Id, orgId);

            var calendar = new Ical.Net.Calendar();
            calendar.Events.Add(calendarEvent);

            var serializedCalendar = new CalendarSerializer().SerializeToString(calendar);
            var calByteArray = Encoding.UTF8.GetBytes(serializedCalendar);
            var emailDto = new EmailDto(emails, $"Invitation: {@event.Name} @ {@event.StartDate.ToString("d")}", "");

            using (var stream = new MemoryStream(calByteArray))
            {
                emailDto.Attachments.Add(new MailAttachment(stream, FileExportName.Sanitize(@event.Name, "invite", ".ics")));
                await _mailingService.SendEmailAsync(emailDto);
            }
        }

        public async Task<FileExportDto> DownloadEventAsync(Guid eventId, int orgId, string userId)
        {
            var @event = await _eventsDbSet.FindAsync(eventId);

            _eventValidationService.CheckIfEventExists(@event);

            var calEvent = new CalendarEvent
            {
                // ReSharper disable once PossibleNullReferenceException
                Uid = @event.Id.ToString(),
                Location = @event.Place,
                Summary = @event.Name,
                Description = @event.Description,
                Organizer = new Organizer { CommonName = BusinessLayerConstants.EmailSenderName, Value = new Uri($"mailto:{BusinessLayerConstants.FromEmailAddress}") },
                Start = new CalDateTime(@event.StartDate, "UTC"),
                End = new CalDateTime(@event.EndDate, "UTC"),
                Status = EventStatus.Confirmed
            };

            AddChoices(calEvent, @event.Name, await GetChosenOptionsAsync(eventId, userId));
            await AddEventLinkToDescriptionAsync(calEvent, eventId, orgId);
            var cal = new Ical.Net.Calendar();
            cal.Events.Add(calEvent);
            var serializedCalendar = new CalendarSerializer().SerializeToString(cal);
            var calByteArray = Encoding.UTF8.GetBytes(serializedCalendar);

            var fileName = FileExportName.Sanitize(@event.Name, "event", ".ics");
            return new FileExportDto(calByteArray, fileName);
        }

        private async Task<List<EventOption>> GetChosenOptionsAsync(Guid eventId, string userId)
        {
            return await _eventParticipantsDbSet
                .Where(participant => participant.EventId == eventId && participant.ApplicationUserId == userId)
                .SelectMany(participant => participant.EventOptions)
                .ToListAsync();
        }

        // Both kinds of pick, flat options before question answers, so the order is stable
        // whatever the event is built from. An answer carries its option name alone: with the
        // question title it would not fit a calendar entry title.
        private static List<string> ChoiceNames(IEnumerable<EventOption> chosenOptions)
        {
            return (chosenOptions ?? Enumerable.Empty<EventOption>())
                .OrderBy(option => option.QuestionId == null ? 0 : 1)
                .ThenBy(option => option.QuestionId)
                .ThenBy(option => option.Order)
                .Select(option => option.Option)
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .ToList();
        }

        private static string Summarize(string eventName, List<string> choices)
        {
            return choices.Count == 0 ? eventName : $"{eventName} — {string.Join(", ", choices)}";
        }

        private static void AddChoices(CalendarEvent calEvent, string eventName, IEnumerable<EventOption> chosenOptions)
        {
            var choices = ChoiceNames(chosenOptions);

            calEvent.Summary = Summarize(eventName, choices);

            if (choices.Count == 0)
            {
                return;
            }

            calEvent.Description += $"\n\nYour choices: {string.Join(", ", choices)}";
        }

        private async Task AddEventLinkToDescriptionAsync(CalendarEvent calEvent, Guid eventId, int orgId)
        {
            var orgShortName = (await _organizationsDbSet.FindAsync(orgId))?.ShortName;
            var eventUrl = _appSettings.EventUrl(orgShortName, eventId.ToString());
            calEvent.Description += $"\n\n{eventUrl}";
        }

        private static CalendarEvent MapToCalendarEvent(EventJoinValidationDto @event)
        {
            var calEvent = new CalendarEvent
            {
                Uid = @event.Id.ToString(),
                Location = @event.Location,
                Summary = @event.Name,
                Description = @event.Description,
                Organizer = new Organizer { CommonName = BusinessLayerConstants.DefaultEmailLinkName, Value = new Uri($"mailto:{BusinessLayerConstants.FromEmailAddress}") },
                Start = new CalDateTime(@event.StartDate, "UTC"),
                End = new CalDateTime(@event.EndDate, "UTC"),
                Status = EventStatus.Confirmed
            };

            return calEvent;
        }
    }
}
