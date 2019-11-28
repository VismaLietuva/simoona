using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.DataTransferObjects.Models.Notification;
using System.Threading.Tasks;
using Shrooms.EntityModels.Models.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Notifications
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateForEvent(UserAndOrganizationDTO userOrg, CreateEventDto eventDto);

        void CreateForEventJoinReminder(EventTypeDTO eventType, IEnumerable<string> usersToNotify, UserAndOrganizationDTO userOrg);
    }
}
