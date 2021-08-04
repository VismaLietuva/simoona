using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        private readonly IDbSet<ApplicationUser> _usersDbSet;

        private readonly DbSet<Event> _eventsDbSet;
        private readonly DbSet<Organization> _organizationsDbSet;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _appSettings;
        private IEventValidationService _eventValidationService;

        public EventCalendarService(IUnitOfWork2 uow, IMailingService mailingService, IApplicationSettings appSettings, IEventValidationService eventValidationService)
        {
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _eventsDbSet = uow.GetDbSet<Event>();
            _organizationsDbSet = uow.GetDbSet<Organization>();
            _mailingService = mailingService;
            _appSettings = appSettings;
            _eventValidationService = eventValidationService;
        }

        public async Task SendInvitationAsync(EventJoinValidationDTO @event, IEnumerable<string> userIds, int orgId)
        {
            var emails = await _usersDbSet
                .Where(u => userIds.Contains(u.Id))
                .Select(u => u.Email)
                .ToListAsync();

            var calendarEvent = MapToCalendarEvent(@event);
            await AddEventLinkToDescriptionAsync(calendarEvent, @event.Id, orgId);

            var calendar = new Ical.Net.Calendar();
            calendar.Events.Add(calendarEvent);

            var serializedCalendar = new CalendarSerializer().SerializeToString(calendar);
            var calByteArray = Encoding.UTF8.GetBytes(serializedCalendar);
            var emailDto = new EmailDto(emails, $"Invitation: {@event.Name} @ {@event.StartDate.ToString("d")}", "");

            using (var stream = new MemoryStream(calByteArray))
            {
                emailDto.Attachment = new MailAttachment(stream, "invite.ics");
                await _mailingService.SendEmailAsync(emailDto);
            }
        }

        public async Task<byte[]> DownloadEventAsync(Guid eventId, int orgId)
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

            await AddEventLinkToDescriptionAsync(calEvent, eventId, orgId);
            var cal = new Ical.Net.Calendar();
            cal.Events.Add(calEvent);
            var serializedCalendar = new CalendarSerializer().SerializeToString(cal);
            var calByteArray = Encoding.UTF8.GetBytes(serializedCalendar);

            return calByteArray;
        }

        private async Task AddEventLinkToDescriptionAsync(CalendarEvent calEvent, Guid eventId, int orgId)
        {
            var orgShortName = (await _organizationsDbSet.FindAsync(orgId))?.ShortName;
            var eventUrl = _appSettings.EventUrl(orgShortName, eventId.ToString());
            calEvent.Description += $"\n\n{eventUrl}";
        }

        private static CalendarEvent MapToCalendarEvent(EventJoinValidationDTO @event)
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
