using Shrooms.EntityModels.Models.Kudos;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Kudos;
using System.Collections.Generic;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Email.Kudos
{
    public interface IKudosPremiumNotificationService
    {
        void SendLoyaltyBotNotification(IEnumerable<AwardedKudosEmployeeDTO> awardedEmployees);
    }
}
