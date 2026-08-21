using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Vacations
{
    /// <summary>
    /// Append-only: a request submitted, re-dated then rejected is three rows
    /// here and one in <see cref="VacationRequest"/>. The request's shape *after*
    /// the action is snapshotted, so the log renders without joining a live row.
    /// </summary>
    public class VacationRequestEvent : BaseModelWithOrg
    {
        public const int MaxCommentLength = 500;

        public int VacationRequestId { get; set; }

        [ForeignKey(nameof(VacationRequestId))]
        public virtual VacationRequest VacationRequest { get; set; }

        public VacationEventKind Kind { get; set; }

        [Required]
        public string ActorId { get; set; }

        [ForeignKey(nameof(ActorId))]
        public virtual ApplicationUser Actor { get; set; }

        public DateTime OccurredAt { get; set; }

        [Required]
        public string EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual ApplicationUser Employee { get; set; }

        public VacationRequestType Type { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public double WorkingDays { get; set; }

        [StringLength(MaxCommentLength)]
        public string Comment { get; set; }

        /// <summary>
        /// JSON array of { field, from, to }, only for Edited. A column rather
        /// than a child table: the list is only ever read whole, for one event.
        /// </summary>
        public string ChangesJson { get; set; }
    }
}
