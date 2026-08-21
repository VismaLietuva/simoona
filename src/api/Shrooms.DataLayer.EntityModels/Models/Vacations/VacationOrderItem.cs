using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Vacations
{
    /// <summary>
    /// One line of a <see cref="VacationOrder"/>. Name, period and type are
    /// snapshotted: an order is a signed document, so a later correction to the
    /// request must not silently change what the printed page says.
    /// </summary>
    public class VacationOrderItem : BaseModel
    {
        public const int MaxEmployeeNameLength = 200;

        public int VacationOrderId { get; set; }

        [ForeignKey(nameof(VacationOrderId))]
        public virtual VacationOrder VacationOrder { get; set; }

        public int VacationRequestId { get; set; }

        [ForeignKey(nameof(VacationRequestId))]
        public virtual VacationRequest VacationRequest { get; set; }

        [Required]
        [StringLength(MaxEmployeeNameLength)]
        public string EmployeeName { get; set; }

        public VacationRequestType Type { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }
    }
}
