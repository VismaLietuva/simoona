using Shrooms.Premium.Domain.Services.Badges;
using Shrooms.Premium.Domain.Services.Books;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Lotteries;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.LoyaltyKudos;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks
{
    public interface IWebHookCallbackPremiumServices
    {
        IEventsWebHookService Events { get; }

        IBookRemindService Books { get; }

        ILoyaltyKudosService LoyaltyKudos { get; }

        IBadgesService BadgesService { get; }

        IEventRemindService EventRemindService { get; }

        ILotteryStatusChangeService LotteryStatusChangeService { get; }
    }
}
