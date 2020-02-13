using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.Host.Contracts.Constants;

namespace Shrooms.EntityModels.Models
{
    public class Exam : BaseModelWithOrg
    {
        [Required]
        [StringLength(ValidationConstants.ExamMaxTitleLength)]
        [Index]
        public string Title { get; set; }

        [StringLength(ValidationConstants.ExamMaxNumberLength)]
        [Index]
        public string Number { get; set; }

        public virtual ICollection<Certificate> Certificates { get; set; }

        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }
    }
}
