using Shrooms.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Domain.Services.WebHookCallbacks.LoyaltyKudos;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.WebHookCallbacks
{
    public class WebHookCallbackServices : IWebHookCallbackServices
    {
        public IEventsWebHookService Events { get; set; }

        public ILoyaltyKudosService LoyaltyKudos { get; set; }
    }
}
