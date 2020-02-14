using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class Project : BaseModelWithOrg
    {
        public string Name { get; set; }

        public string Desc { get; set; }

        [ForeignKey("Owner")]
        public string OwnerId { get; set; }

        public virtual ApplicationUser Owner { get; set; }

        public virtual ICollection<ApplicationUser> Members { get; set; }

        public virtual ICollection<Skill> Attributes { get; set; }

        [ForeignKey("Wall")]
        public int WallId { get; set; }

        public virtual Wall Wall { get; set; }

        public string Logo { get; set; }
    }
}
