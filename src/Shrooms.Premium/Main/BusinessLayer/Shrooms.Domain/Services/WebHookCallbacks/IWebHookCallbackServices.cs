using Shrooms.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Domain.Services.WebHookCallbacks.LoyaltyKudos;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.WebHookCallbacks
{
    public interface IWebHookCallbackServices
    {
        IEventsWebHookService Events { get; set; }

        ILoyaltyKudosService LoyaltyKudos { get; set; }
    }
}
