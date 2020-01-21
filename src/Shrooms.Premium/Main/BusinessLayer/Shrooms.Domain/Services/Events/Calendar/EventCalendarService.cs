using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models.Emails;
using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Events;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Infrastructure.Email;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;

namespace Shrooms.Domain.Services.Events.Calendar
{
    public class EventCalendarService : IEventCalendarService
    {
        private readonly IDbSet<ApplicationUser> _usersDbSet;

        private readonly IDbSet<Event> _eventsDbSet;
        private readonly IDbSet<Organization> _organizationsDbSet;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _appSettings;

        public EventCalendarService(IUnitOfWork2 uow, IMailingService mailingService, IApplicationSettings appSettings)
        {
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _eventsDbSet = uow.GetDbSet<Event>();
            _organizationsDbSet = uow.GetDbSet<Organization>();
            _mailingService = mailingService;
            _appSettings = appSettings;
        }

        public void SendInvitation(EventJoinValidationDTO @event, IEnumerable<string> userIds, int orgId)
        {
            var emails = _usersDbSet
                .Where(u => userIds.Contains(u.Id))
                .Select(u => u.Email)
                .ToList();

            var calendarEvent = MapToCalendarEvent(@event);
            AddEventLinkToDescription(calendarEvent, @event.Id, orgId);
            var calendar = new Ical.Net.Calendar();
            calendar.Events.Add(calendarEvent);
            var serializedCalendar = new CalendarSerializer().SerializeToString(calendar);
            var calByteArray = Encoding.UTF8.GetBytes(serializedCalendar);
            var emailDto = new EmailDto(emails, $"Invitation: {@event.Name} @ {@event.StartDate}", "");

            using (var stream = new MemoryStream(calByteArray))
            {
                emailDto.Attachment = new System.Net.Mail.Attachment(stream, "invite.ics");
                _mailingService.SendEmail(emailDto);
            }
        }

        public byte[] DownloadEvent(Guid eventId, int orgId)
        {
            var @event = _eventsDbSet.Find(eventId);

            var calEvent = new CalendarEvent
            {
                Uid = @event.Id.ToString(),
                Location = @event.Place,
                Summary = @event.Name,
                Description = @event.Description,
                Organizer = new Organizer { CommonName = ConstBusinessLayer.EmailSenderName, Value = new Uri($"mailto:{ConstBusinessLayer.FromEmailAddress}") },
                Start = new CalDateTime(@event.StartDate, "UTC"),
                End = new CalDateTime(@event.EndDate, "UTC"),
                Status = EventStatus.Confirmed
            };

            AddEventLinkToDescription(calEvent, eventId, orgId);
            var cal = new Ical.Net.Calendar();
            cal.Events.Add(calEvent);
            var serializedCalendar = new CalendarSerializer().SerializeToString(cal);
            var calByteArray = Encoding.UTF8.GetBytes(serializedCalendar);

            return calByteArray;
        }

        private void AddEventLinkToDescription(CalendarEvent calEvent, Guid eventId, int orgId)
        {
            var orgShortName = _organizationsDbSet.Find(orgId).ShortName;
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
                Organizer = new Organizer { CommonName = ConstBusinessLayer.DefaultEmailLinkName, Value = new Uri($"mailto:{ConstBusinessLayer.FromEmailAddress}") },
                Start = new CalDateTime(@event.StartDate, "UTC"),
                End = new CalDateTime(@event.EndDate, "UTC"),
                Status = EventStatus.Confirmed,
            };

            return calEvent;
        }
    }
}