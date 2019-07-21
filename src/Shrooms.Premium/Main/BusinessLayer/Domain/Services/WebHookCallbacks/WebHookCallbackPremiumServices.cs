using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Badges;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.LoyaltyKudos;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks
{
    public class WebHookCallbackPremiumServices : IWebHookCallbackPremiumServices
    {
        public IEventsWebHookService Events { get; set; }

        public ILoyaltyKudosService LoyaltyKudos { get; set; }

        public IBadgesService BadgesService { get; set; }
    }
}
