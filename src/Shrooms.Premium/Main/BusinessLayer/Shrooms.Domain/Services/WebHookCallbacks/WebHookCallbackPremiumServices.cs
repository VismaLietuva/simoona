using Shrooms.Domain.Services.Badges;
using Shrooms.Domain.Services.Books;
using Shrooms.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Domain.Services.WebHookCallbacks.LoyaltyKudos;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.WebHookCallbacks
{
    public class WebHookCallbackPremiumServices : IWebHookCallbackPremiumServices
    {
        public IEventsWebHookService Events { get; set; }

        public IBookRemindService Books { get; set; }

        public ILoyaltyKudosService LoyaltyKudos { get; set; }

        public IBadgesService BadgesService { get; set; }

        public IEventJoinRemindService EventJoinRemindService { get; set; }
    }
}
