using Shrooms.EntityModels.Models.Kudos;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Kudos
{
    public interface IKudosPremiumNotificationService
    {
        void SendLoyaltyBotNotification(KudosLog kudosLog);
    }
}
