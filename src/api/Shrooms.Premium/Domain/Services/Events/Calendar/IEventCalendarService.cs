using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Events.Calendar
{
    public interface IEventCalendarService
    {
        Task SendInvitationAsync(EventJoinValidationDto @event, IEnumerable<string> userIds, int orgId);

        Task<byte[]> DownloadEventAsync(Guid eventId, int orgId);
    }
}