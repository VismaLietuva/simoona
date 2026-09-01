using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models.Vacations
{
    /// <summary>
    /// One public holiday. Deliberately not a <see cref="BaseModelWithOrg"/>:
    /// there is no surrogate key, no audit trail and no organisation — the row
    /// *is* its key, and the calendar is national rather than per-tenant.
    ///
    /// A second country would need a composite key and a country column. Until a
    /// tenant outside Lithuania exists, the extra column would only be a column
    /// with one value in it.
    /// </summary>
    public class Holiday
    {
        public const int MaxNameLength = 100;

        [Key]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(MaxNameLength)]
        public string Name { get; set; }
    }
}
