using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.EntityModels.Models.Events;
using System;
using System.Collections.Generic;

namespace Shrooms.Domain.Services.Events.Calendar
{
    public interface IEventCalendarService
    {
        void DeleteEvent(Guid eventId, int orgId);

        void UpdateEvent(Event @event, int orgId);

        void CreateEvent(Event @event, int orgId);

        void ResetParticipants(Guid eventId, int orgId);

        void RemoveParticipants(Guid eventId, int orgId, IEnumerable<string> userIds);

        void AddParticipants(Guid eventId, int orgId, IEnumerable<string> userIds, IEnumerable<string> choices);

        void SendInvitation(EventJoinValidationDTO @event, IEnumerable<string> userIds, int orgId);

        byte[] DownloadEvent(Guid eventId, int orgId);
    }
}