using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.EntityModels.Models.Events;
using System;
using System.Collections.Generic;

namespace Shrooms.Domain.Services.Events.Calendar
{
    public interface IEventCalendarService
    {
        void SendInvitation(EventJoinValidationDTO @event, IEnumerable<string> userIds, int orgId);

        byte[] DownloadEvent(Guid eventId, int orgId);
    }
}