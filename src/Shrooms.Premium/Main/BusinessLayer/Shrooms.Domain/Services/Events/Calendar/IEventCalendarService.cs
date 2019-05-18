using System;
using System.Collections.Generic;
using Shrooms.EntityModels.Models.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Events.Calendar
{
    public interface IEventCalendarService
    {
        void DeleteEvent(Guid eventId, int orgId);

        void UpdateEvent(Event @event, int orgId);

        void CreateEvent(Event @event, int orgId);

        void ResetParticipants(Guid eventId, int orgId);

        void RemoveParticipants(Guid eventId, int orgId, IEnumerable<string> userIds);

        void AddParticipants(Guid eventId, int orgId, IEnumerable<string> userIds, IEnumerable<string> choices);
    }
}