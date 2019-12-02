using Shrooms.DataTransferObjects.Models;

namespace Shrooms.Domain.Services.WebHookCallbacks.Events
{
    public interface IEventJoinRemindService
    {
        void SendNotifications(UserAndOrganizationDTO userOrg);
    }
}
