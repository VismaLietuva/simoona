using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;

namespace Shrooms.DataLayer.EntityModels.Models
{
    [Index(nameof(Title))]
    [Index(nameof(Number))]
    public class Exam : BaseModelWithOrg
    {
        [Required]
        [StringLength(ValidationConstants.ExamMaxTitleLength)]
        public string Title { get; set; }

        [StringLength(ValidationConstants.ExamMaxNumberLength)]
        public string Number { get; set; }

        public virtual ICollection<Certificate> Certificates { get; set; }

        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }
    }
}
