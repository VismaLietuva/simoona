using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Vacations
{
    public class ParentalEntitlement : BaseModelWithOrg
    {
        [Required]
        public string EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual ApplicationUser Employee { get; set; }

        public ParentalEntitlementType Type { get; set; }
    }
}
