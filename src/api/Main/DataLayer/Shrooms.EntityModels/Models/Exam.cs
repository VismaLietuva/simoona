using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.EntityModels.Models
{
    public class Exam : BaseModelWithOrg
    {
        public const int MaxTitleLength = 255;
        public const int MaxNumberLength = 255;

        [Required]
        [StringLength(MaxTitleLength)]
        [Index]
        public string Title { get; set; }

        [StringLength(MaxNumberLength)]
        [Index]
        public string Number { get; set; }

        public virtual ICollection<Certificate> Certificates { get; set; }

        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }
    }
}
