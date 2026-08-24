using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Vacations
{
    /// <summary>
    /// The current state of one leave request. Mutated in place — the history of
    /// how it got here lives in <see cref="VacationRequestEvent"/>.
    /// </summary>
    public class VacationRequest : BaseModelWithOrg
    {
        public const int MaxNoteLength = 500;
        public const int MaxReviewCommentLength = 500;

        [Required]
        public string EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual ApplicationUser Employee { get; set; }

        public VacationRequestType Type { get; set; }

        public VacationRequestStatus Status { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        /// <summary>
        /// Denormalised working-day count. Recomputed on every write — the
        /// client's value is never trusted.
        /// </summary>
        public double WorkingDays { get; set; }

        [StringLength(MaxNoteLength)]
        public string Note { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string ReviewedById { get; set; }

        [ForeignKey(nameof(ReviewedById))]
        public virtual ApplicationUser ReviewedBy { get; set; }

        [StringLength(MaxReviewCommentLength)]
        public string ReviewComment { get; set; }
    }
}
