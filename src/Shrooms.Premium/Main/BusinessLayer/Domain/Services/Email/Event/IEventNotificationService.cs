using System;
using System.Collections.Generic;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Event
{
    public interface IEventNotificationService
    {
        void NotifyRemovedEventParticipants(string eventName, Guid eventId, int orgId, IEnumerable<string> users);

    }
}
