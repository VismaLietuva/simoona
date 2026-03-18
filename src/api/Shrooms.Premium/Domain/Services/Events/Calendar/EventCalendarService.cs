using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;

namespace Shrooms.Premium.Domain.Services.Events.Calendar
{
    /// <summary>
    /// Service for managing event calendar operations such as sending invitations and downloading events.
    /// </summary>
    public class EventCalendarService : IEventCalendarService
    {
        private readonly DbSet<ApplicationUser> usersDbSet;
        private readonly DbSet<Event> eventsDbSet;
        private readonly DbSet<Organization> organizationsDbSet;
        private readonly IMailingService mailingService;
        private readonly IApplicationSettings appSettings;
        private readonly IEventValidationService eventValidationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventCalendarService"/> class.
        /// </summary>
        public EventCalendarService(IUnitOfWork2 uow, IMailingService mailingService, IApplicationSettings appSettings, IEventValidationService eventValidationService)
        {
            this.usersDbSet = uow.GetDbSet<ApplicationUser>();
            this.eventsDbSet = uow.GetDbSet<Event>();
            this.organizationsDbSet = uow.GetDbSet<Organization>();
            this.mailingService = mailingService;
            this.appSettings = appSettings;
            this.eventValidationService = eventValidationService;
        }

        /// <summary>
        /// Sends a calendar invitation email to the specified users for the given event.
        /// </summary>
        public async Task SendInvitationAsync(EventJoinValidationDto @event, IEnumerable<string> userIds, int orgId)
        {
            var emails = await this.usersDbSet
                .Where(u => userIds.Contains(u.Id))
                .Select(u => u.Email)
                .ToListAsync();

            var calendarEvent = MapToCalendarEvent(@event);
            await this.AddEventLinkToDescriptionAsync(calendarEvent, @event.Id, orgId);

            var calendar = new Ical.Net.Calendar();
            calendar.Events.Add(calendarEvent);

            var serializedCalendar = new CalendarSerializer().SerializeToString(calendar);
            var calByteArray = Encoding.UTF8.GetBytes(serializedCalendar);
            var emailDto = new EmailDto(emails, $"Invitation: {@event.Name} @ {@event.StartDate.ToString("d")}", string.Empty);

            var attachment = new MimePart("text", "calendar")
            {
                Content = new MimeContent(new MemoryStream(calByteArray)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = "invite.ics",
            };
            emailDto.Attachment = attachment;
            await this.mailingService.SendEmailAsync(emailDto);
        }

        /// <summary>
        /// Downloads the calendar event data as a byte array for the specified event.
        /// </summary>
        public async Task<byte[]> DownloadEventAsync(Guid eventId, int orgId)
        {
            var @event = await this.eventsDbSet.FindAsync(eventId);

            this.eventValidationService.CheckIfEventExists(@event);

            var calEvent = new CalendarEvent
            {
                Uid = @event!.Id.ToString(),
                Location = @event.Place,
                Summary = @event.Name,
                Description = @event.Description,
                Organizer = new Organizer { CommonName = BusinessLayerConstants.EmailSenderName, Value = new Uri($"mailto:{BusinessLayerConstants.FromEmailAddress}") },
                Start = new CalDateTime(@event.StartDate, "UTC"),
                End = new CalDateTime(@event.EndDate, "UTC"),
                Status = EventStatus.Confirmed,
            };

            await this.AddEventLinkToDescriptionAsync(calEvent, eventId, orgId);
            var cal = new Ical.Net.Calendar();
            cal.Events.Add(calEvent);
            var serializedCalendar = new CalendarSerializer().SerializeToString(cal);
            var calByteArray = Encoding.UTF8.GetBytes(serializedCalendar);

            return calByteArray;
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
                Status = EventStatus.Confirmed,
            };

            return calEvent;
        }

        private async Task AddEventLinkToDescriptionAsync(CalendarEvent calEvent, Guid eventId, int orgId)
        {
            var orgShortName = (await this.organizationsDbSet.FindAsync(orgId))?.ShortName;
            var eventUrl = this.appSettings.EventUrl(orgShortName, eventId.ToString());
            calEvent.Description += $"\n\n{eventUrl}";
        }
    }
}
