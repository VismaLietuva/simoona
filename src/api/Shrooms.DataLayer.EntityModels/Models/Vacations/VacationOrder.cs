using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Vacations
{
    /// <summary>
    /// A signed leave order ("Įsakymas dėl kasmetinių atostogų suteikimo"). The
    /// number is allocated once and never reused, so a reprint reproduces the
    /// paper that was actually issued.
    /// </summary>
    public class VacationOrder : BaseModelWithOrg
    {
        public const int MaxPrefixLength = 10;

        public int Number { get; set; }

        /// <summary>Snapshotted from settings so a later prefix change cannot rewrite history.</summary>
        [Required]
        [StringLength(MaxPrefixLength)]
        public string Prefix { get; set; }

        public DateTime IssuedOn { get; set; }

        /// <summary>
        /// The leave type and start day this order covers — one order per day per
        /// type, which is how payroll files them. Together they are the key a
        /// regeneration matches on, so the number survives it. Null on an order
        /// assembled by hand out of several days or types.
        /// </summary>
        public VacationRequestType? Type { get; set; }

        public DateTime? PeriodStart { get; set; }

        [Required]
        public string IssuedById { get; set; }

        [ForeignKey(nameof(IssuedById))]
        public virtual ApplicationUser IssuedBy { get; set; }

        public virtual ICollection<VacationOrderItem> Items { get; set; }

        [NotMapped]
        public string Reference => $"{Prefix}{Number}";
    }
}
