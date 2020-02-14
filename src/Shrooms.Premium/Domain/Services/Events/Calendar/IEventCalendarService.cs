using System;
using System.Collections.Generic;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Events.Calendar
{
    public interface IEventCalendarService
    {
        void SendInvitation(EventJoinValidationDTO @event, IEnumerable<string> userIds, int orgId);

        byte[] DownloadEvent(Guid eventId, int orgId);
    }
}