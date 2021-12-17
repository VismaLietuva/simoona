using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;

namespace Shrooms.Premium.Domain.Services.Email.Kudos
{
    public interface IKudosPremiumNotificationService
    {
        Task SendLoyaltyBotNotificationAsync(IEnumerable<AwardedKudosEmployeeDto> awardedEmployees);
    }
}
