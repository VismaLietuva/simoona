using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Badges;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.LoyaltyKudos;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks
{
    public interface IWebHookCallbackPremiumServices
    {
        IEventsWebHookService Events { get; set; }

        ILoyaltyKudosService LoyaltyKudos { get; set; }

        IBadgesService BadgesService { get; set; }
    }
}
