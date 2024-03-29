using System.Collections.Generic;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Premium.DataTransferObjects.Models.Kudos
{
    public class EmployeeLoyaltyKudosDto
    {
        public string EmployeeId { get; set; }

        public ApplicationUser Employee { get; set; }

        public IEnumerable<int> AwardedEmploymentYears { get; set; }

        public int AwardedLoyaltyKudosCount { get; set; }
    }
}