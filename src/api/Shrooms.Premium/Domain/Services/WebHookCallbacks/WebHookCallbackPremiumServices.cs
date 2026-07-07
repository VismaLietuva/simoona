using Shrooms.Premium.Domain.Services.Badges;
using Shrooms.Premium.Domain.Services.Books;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Lotteries;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.LoyaltyKudos;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks
{
    public class WebHookCallbackPremiumServices : IWebHookCallbackPremiumServices
    {
        public IEventsWebHookService Events { get; }

        public IBookRemindService Books { get; }

        public ILoyaltyKudosService LoyaltyKudos { get; }

        public IBadgesService BadgesService { get; }

        public IEventRemindService EventRemindService { get; }

        public ILotteryStatusChangeService LotteryStatusChangeService { get; }

        public WebHookCallbackPremiumServices(
            IEventsWebHookService events,
            IBookRemindService books,
            ILoyaltyKudosService loyaltyKudos,
            IBadgesService badgesService,
            IEventRemindService eventRemindService,
            ILotteryStatusChangeService lotteryStatusChangeService)
        {
            Events = events;
            Books = books;
            LoyaltyKudos = loyaltyKudos;
            BadgesService = badgesService;
            EventRemindService = eventRemindService;
            LotteryStatusChangeService = lotteryStatusChangeService;
        }
    }
}
