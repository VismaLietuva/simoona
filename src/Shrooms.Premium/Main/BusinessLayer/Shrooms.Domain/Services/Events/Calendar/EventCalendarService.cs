using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models.CalendarEvent;
using Shrooms.DataTransferObjects.Models.Emails;
using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Events;
using Shrooms.Infrastructure.Calendar;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.FireAndForget;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Shrooms.Domain.Services.Events.Calendar
{
    public class EventCalendarService : IEventCalendarService
    {
        private readonly IDbSet<ApplicationUser> _usersDbSet;

        private readonly IJobScheduler _scheduler;
        private readonly IDbSet<Organization> _organizationsDbSet;
        private readonly IMailingService _mailingService;

        public EventCalendarService(IUnitOfWork2 uow, IJobScheduler scheduler, IMailingService mailingService)
        {
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationsDbSet = uow.GetDbSet<Organization>();
            _scheduler = scheduler;
            _mailingService = mailingService;
        }

        public void DeleteEvent(Guid eventId, int orgId)
        {
            var calendarId = GetOrgCalendarId(orgId);
            _scheduler.Enqueue<GoogleCalendarService>(x => x.DeleteEvent(eventId, calendarId));
        }

        public void UpdateEvent(Event @event, int orgId)
        {
            var calendarId = GetOrgCalendarId(orgId);
            var calendarEvent = MapToCalendarEventDto(@event);
            _scheduler.Enqueue<GoogleCalendarService>(x => x.UpdateEvent(calendarEvent, calendarId));
        }

        public void CreateEvent(Event @event, int orgId)
        {
            var calendarId = GetOrgCalendarId(orgId);
            var calendarEvent = MapToCalendarEventDto(@event);
            _scheduler.Enqueue<GoogleCalendarService>(x => x.CreateEvent(calendarEvent, calendarId));
        }

        public void AddParticipants(Guid eventId, int orgId, IEnumerable<string> userIds, IEnumerable<string> choices)
        {
            var calendarId = GetOrgCalendarId(orgId);
            var emails = _usersDbSet
                .Where(u => userIds.Contains(u.Id))
                .Select(u => u.Email)
                .ToList();

            _scheduler.Enqueue<GoogleCalendarService>(x => x.AddParticipants(eventId, calendarId, emails, choices));
        }

        public void RemoveParticipants(Guid eventId, int orgId, IEnumerable<string> userIds)
        {
            var calendarId = GetOrgCalendarId(orgId);
            var emails = _usersDbSet
                .Where(u => userIds.Contains(u.Id))
                .Select(u => u.Email)
                .ToList();

            _scheduler.Enqueue<GoogleCalendarService>(x => x.RemoveParticipants(eventId, emails, calendarId));
        }

        public void ResetParticipants(Guid eventId, int orgId)
        {
            var calendarId = GetOrgCalendarId(orgId);
            _scheduler.Enqueue<GoogleCalendarService>(x => x.ResetParticipants(eventId, calendarId));
        }

        public void SendInvitation(EventJoinValidationDTO @event, IEnumerable<string> userIds)
        {
            var emails = _usersDbSet
                .Where(u => userIds.Contains(u.Id))
                .Select(u => u.Email)
                .ToList();

            var calendarEvent = MapToCalendarEvent(@event);
            var calendar = new Ical.Net.Calendar();
            calendar.Events.Add(calendarEvent);
            var serializedCalendar = new CalendarSerializer().SerializeToString(calendar);
            byte[] calByteArray = Encoding.UTF8.GetBytes(serializedCalendar);
            var emailDto = new EmailDto(emails, $"Invitation: {@event.Name} @ {@event.StartDate}", "");

            using (MemoryStream stream = new MemoryStream(calByteArray))
            {
                emailDto.Attachment = new System.Net.Mail.Attachment(stream, "invite.ics");
                _mailingService.SendEmail(emailDto);
            }
        }

        private CalendarEventDTO MapToCalendarEventDto(Event @event)
        {
            var calendarEvent = new CalendarEventDTO()
            {
                EventId = @event.Id,
                Description = @event.Description,
                StartDate = DateTime.SpecifyKind(@event.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(@event.EndDate, DateTimeKind.Utc),
                Name = @event.Name,
                Location = @event.Place,
            };

            return calendarEvent;
        }

        private CalendarEvent MapToCalendarEvent(EventJoinValidationDTO @event)
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
                Status = EventStatus.Confirmed
            };

            return calEvent;
        }

        private string GetOrgCalendarId(int orgId)
        {
            var calendarId = _organizationsDbSet
                .Where(o => o.Id == orgId)
                .Select(o => o.CalendarId)
                .First();

            return calendarId;
        }
    }
}