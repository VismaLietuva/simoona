using Shrooms.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Domain.Services.WebHookCallbacks.LoyaltyKudos;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.WebHookCallbacks
{
    public interface IWebHookCallbackPremiumServices
    {
        IEventsWebHookService Events { get; set; }

        ILoyaltyKudosService LoyaltyKudos { get; set; }
    }
}
