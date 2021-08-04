using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Email.Event
{
    public interface IEventNotificationService
    {
        Task NotifyRemovedEventParticipantsAsync(string eventName, Guid eventId, int orgId, IEnumerable<string> users);

        Task RemindUsersToJoinEventAsync(IEnumerable<EventTypeDto> eventType, IEnumerable<string> users, int orgId);
    }
}
