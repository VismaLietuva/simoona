using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks.Events
{
    public interface IEventRemindService
    {
        Task SendNotificationsAsync(string organizationName);

        Task SendJoinedNotificationsAsync(string organizationName);
    }
}
