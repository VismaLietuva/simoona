using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Notification;
using System.Threading.Tasks;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Notifications
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateForEvent(UserAndOrganizationDTO userOrg, CreateEventDto eventDto);
    }
}
