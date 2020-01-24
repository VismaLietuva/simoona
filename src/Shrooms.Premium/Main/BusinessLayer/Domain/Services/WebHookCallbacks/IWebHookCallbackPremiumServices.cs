using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Badges;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Books;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.LoyaltyKudos;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks
{
    public interface IWebHookCallbackPremiumServices
    {
        IEventsWebHookService Events { get; set; }

        IBookRemindService Books { get; set; }

        ILoyaltyKudosService LoyaltyKudos { get; set; }

        IBadgesService BadgesService { get; set; }

        IEventJoinRemindService EventJoinRemindService { get; set; }
    }
}
