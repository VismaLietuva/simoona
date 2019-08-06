using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Kudos;
using System.Collections.Generic;

namespace Shrooms.Domain.Services.WebHookCallbacks.LoyaltyKudos
{
    public interface ILoyaltyKudosService
    {
        List<AwardedKudosEmployeeDTO> AwardEmployeesWithKudos(string organizationName);
    }
}
