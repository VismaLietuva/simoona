using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Events;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Premium.Infrastructure.Shrooms.Infrastructure.Calendar;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Calendar;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Events.Calendar
{
    public class EventCalendarService : IEventCalendarService
    {
        private readonly IDbSet<ApplicationUser> _usersDbSet;

        private readonly IJobScheduler _scheduler;
        private readonly IDbSet<Organization> _organizationsDbSet;

        public EventCalendarService(IUnitOfWork2 uow, IJobScheduler scheduler)
        {
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationsDbSet = uow.GetDbSet<Organization>();
            _scheduler = scheduler;
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