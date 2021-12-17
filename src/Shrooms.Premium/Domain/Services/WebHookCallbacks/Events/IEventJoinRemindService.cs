using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks.Events
{
    public interface IEventJoinRemindService
    {
        Task SendNotificationsAsync(string orgName);
    }
}
