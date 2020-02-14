using System.Collections.Generic;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;

namespace Shrooms.Premium.Domain.Services.Email.Kudos
{
    public interface IKudosPremiumNotificationService
    {
        void SendLoyaltyBotNotification(IEnumerable<AwardedKudosEmployeeDTO> awardedEmployees);
    }
}
