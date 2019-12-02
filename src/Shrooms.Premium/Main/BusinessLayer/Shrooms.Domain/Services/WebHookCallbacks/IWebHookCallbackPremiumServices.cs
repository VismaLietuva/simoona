using Shrooms.Domain.Services.Badges;
using Shrooms.Domain.Services.Books;
using Shrooms.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Domain.Services.WebHookCallbacks.LoyaltyKudos;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.WebHookCallbacks
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
