using Shrooms.Premium.Domain.Services.Badges;
using Shrooms.Premium.Domain.Services.Books;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Lotteries;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.LoyaltyKudos;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks
{
    public class WebHookCallbackPremiumServices : IWebHookCallbackPremiumServices
    {
        public IEventsWebHookService Events { get; set; }

        public IBookRemindService Books { get; set; }

        public ILoyaltyKudosService LoyaltyKudos { get; set; }

        public IBadgesService BadgesService { get; set; }

        public IEventRemindService EventRemindService { get; set; }

        public ILotteryStatusChangeService LotteryStatusChangeService { get; set; }
    }
}
