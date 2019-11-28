using Shrooms.DataTransferObjects.Models;

namespace Shrooms.Domain.Services.WebHookCallbacks
{
    public interface IEventJoinRemindService
    {
        void Notify(UserAndOrganizationDTO userOrg);
    }
}
