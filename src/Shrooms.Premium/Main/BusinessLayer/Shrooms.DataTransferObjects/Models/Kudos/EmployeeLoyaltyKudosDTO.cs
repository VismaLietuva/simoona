using System.Collections.Generic;
using Shrooms.EntityModels.Models;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Kudos
{
    public class EmployeeLoyaltyKudosDTO
    {
        public string EmployeeId { get; set; }

        public ApplicationUser Employee { get; set; }

        public IEnumerable<int> AwardedEmploymentYears { get; set; }

        public int AwardedLoyaltyKudosCount { get; set; }
    }
}