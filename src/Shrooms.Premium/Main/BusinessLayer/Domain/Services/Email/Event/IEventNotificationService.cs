using System;
using System.Collections.Generic;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Event
{
    public interface IEventNotificationService
    {
        void NotifyRemovedEventParticipants(string eventName, Guid eventId, int orgId, IEnumerable<string> users);

        void RemindUsersToJoinEvent(EventTypeDTO eventType, IEnumerable<string> users, int orgId);
    }
}
