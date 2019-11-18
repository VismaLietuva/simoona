using System.Collections.Generic;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Kudos;

namespace Shrooms.Domain.Services.WebHookCallbacks.LoyaltyKudos
{
    public interface ILoyaltyKudosCalculator
    {
        KudosLog CreateLoyaltyKudosLog(ApplicationUser recipient, KudosType loyaltyKudosType, int organizationId, int[] kudosYearlyMultipliers, int yearOfEmployment);

        IEnumerable<int> CalculateYearsToAwardFor(int yearsEmployed, int receivedLoyalties);

        List<KudosLog> GetEmployeeLoyaltyKudosLog(EmployeeLoyaltyKudosDTO employeeLoyaltyKudos,
                                                  KudosType loyaltyType,
                                                  int organizationId,
                                                  int[] kudosYearlyMultipliers);
    }
}