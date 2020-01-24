using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Notification;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Notifications
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateForEvent(UserAndOrganizationDTO userOrg, CreateEventDto eventDto);
        void CreateForEventJoinReminder(EventTypeDTO eventType, IEnumerable<string> usersToNotify, int orgId);
    }
}
