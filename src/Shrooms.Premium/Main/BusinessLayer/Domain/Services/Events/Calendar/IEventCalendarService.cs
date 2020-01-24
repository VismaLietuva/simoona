using System;
using System.Collections.Generic;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.Calendar
{
    public interface IEventCalendarService
    {
        void SendInvitation(EventJoinValidationDTO @event, IEnumerable<string> userIds, int orgId);

        byte[] DownloadEvent(Guid eventId, int orgId);
    }
}