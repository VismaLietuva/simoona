using System.Collections.Generic;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Kudos;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.LoyaltyKudos
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