using Shrooms.DataTransferObjects.Models.CalendarEvent;
using System;
using System.Collections.Generic;

namespace Shrooms.Infrastructure.Calendar
{
    public interface ICalendarService
    {
        void DeleteEvent(Guid eventId, string tenantCalendarId);

        void ResetParticipants(Guid eventId, string tenantCalendarId);

        void CreateEvent(CalendarEventDTO calendarEvent, string tenantCalendarId);

        void UpdateEvent(CalendarEventDTO calendarEventDto, string tenantCalendarId);

        void RemoveParticipants(Guid eventId, IEnumerable<string> emails, string tenantCalendarId);

        void AddParticipants(Guid eventId, string tenantCalendarId, IEnumerable<string> emails, IEnumerable<string> eventChoices);
    }
}