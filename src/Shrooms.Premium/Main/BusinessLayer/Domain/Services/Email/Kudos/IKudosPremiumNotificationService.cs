using System.Collections.Generic;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Kudos;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Kudos
{
    public interface IKudosPremiumNotificationService
    {
        void SendLoyaltyBotNotification(IEnumerable<AwardedKudosEmployeeDTO> awardedEmployees);
    }
}
