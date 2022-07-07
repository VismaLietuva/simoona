using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks.Lotteries
{
    public interface ILotteryStatusChangeService
    {
        Task UpdateStartedLotteriesToEndedAsync();
    }
}
