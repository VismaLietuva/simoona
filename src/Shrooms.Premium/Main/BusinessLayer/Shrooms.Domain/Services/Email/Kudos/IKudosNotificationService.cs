using Shrooms.EntityModels.Models.Kudos;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Email.Kudos
{
    public interface IKudosNotificationService
    {
        void SendLoyaltyBotNotification(KudosLog kudosLog);
    }
}
