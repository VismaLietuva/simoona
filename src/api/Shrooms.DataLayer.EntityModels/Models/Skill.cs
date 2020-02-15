using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class Skill : BaseModel
    {
        [Required]
        [StringLength(200)]
        [Index]
        public string Title { get; set; }

        public bool ShowInAutoComplete { get; set; }

        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }

        public virtual ICollection<Project> Projects { get; set; }
    }
}
