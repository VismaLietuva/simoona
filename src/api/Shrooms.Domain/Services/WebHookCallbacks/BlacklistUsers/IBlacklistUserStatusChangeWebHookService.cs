using System.Threading.Tasks;

namespace Shrooms.Domain.Services.WebHookCallbacks.BlacklistUsers
{
    public interface IBlacklistUserStatusChangeWebHookService
    {
        Task ProcessExpiredBlacklistUsersAsync();
    }
}
