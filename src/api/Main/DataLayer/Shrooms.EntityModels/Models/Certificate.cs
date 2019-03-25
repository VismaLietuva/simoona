using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.EntityModels.Models
{
    public class Certificate : AbstractClassifier
    {
        public virtual ICollection<Exam> Exams { get; set; }

        public bool InProgress { get; set; }

        [ForeignKey("ApplicationUser")]
        public string ApplicationUserId { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}