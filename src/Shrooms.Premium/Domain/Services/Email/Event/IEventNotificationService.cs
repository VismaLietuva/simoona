using System;
using System.Collections.Generic;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Email.Event
{
    public interface IEventNotificationService
    {
        void NotifyRemovedEventParticipants(string eventName, Guid eventId, int orgId, IEnumerable<string> users);

        void RemindUsersToJoinEvent(IEnumerable<EventTypeDTO> eventType, IEnumerable<string> users, int orgId);
    }
}
