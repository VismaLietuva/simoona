using Shrooms.Premium.Domain.Services.Badges;
using Shrooms.Premium.Domain.Services.Books;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Lotteries;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.LoyaltyKudos;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks
{
    public interface IWebHookCallbackPremiumServices
    {
        IEventsWebHookService Events { get; set; }

        IBookRemindService Books { get; set; }

        ILoyaltyKudosService LoyaltyKudos { get; set; }

        IBadgesService BadgesService { get; set; }

        IEventRemindService EventRemindService { get; set; }

        ILotteryStatusChangeService LotteryStatusChangeService { get; set; }
    }
}
