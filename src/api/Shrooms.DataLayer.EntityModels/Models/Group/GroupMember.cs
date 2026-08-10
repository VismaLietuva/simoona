using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models.Group
{
    /// <summary>
    /// A person's membership of a group, with the period they belong(ed) for.
    /// Both dates are optional - an open-ended membership has neither.
    /// </summary>
    /// <remarks>
    /// Keyed on a surrogate Id rather than (GroupId, UserId): the same person can hold
    /// several memberships of one group - separate stints, each with its own period.
    /// </remarks>
    public class GroupMember
    {
        public int Id { get; set; }

        [ForeignKey("Group")]
        public int GroupId { get; set; }

        public virtual Group Group { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        /// <summary>
        /// What this person does in the group during this membership.
        /// </summary>
        public string Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The membership has begun, so it is a matter of record rather than a plan.
        /// A membership with no start date is open-ended and counts as begun, matching
        /// how <see cref="IsActiveDuring"/> reads a missing start date.
        /// </summary>
        public bool HasStarted(DateTime asOf) => !StartDate.HasValue || StartDate.Value <= asOf;

        /// <summary>
        /// True when the membership overlaps the given period at all - someone who was
        /// a member for any part of the month counts as a member for that month.
        /// </summary>
        public bool IsActiveDuring(DateTime periodStart, DateTime periodEnd)
        {
            return (StartDate == null || StartDate <= periodEnd)
                && (EndDate == null || EndDate >= periodStart);
        }
    }
}
