using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Notification;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Notifications
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateForEventAsync(UserAndOrganizationDto userOrg, CreateEventDto eventDto);
        Task CreateForEventJoinReminderAsync(IEnumerable<string> usersToNotify, int orgId);
    }
}
