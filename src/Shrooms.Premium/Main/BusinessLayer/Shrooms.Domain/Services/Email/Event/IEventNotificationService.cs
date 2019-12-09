using System;
using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models.Events;

namespace Shrooms.Domain.Services.Email.Event
{
    public interface IEventNotificationService
    {
        void NotifyRemovedEventParticipants(string eventName, Guid eventId, int orgId, IEnumerable<string> users);

        void RemindUsersToJoinEvent(EventTypeDTO eventType, IEnumerable<string> users, int orgId);
    }
}
