using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.EntityModels.Models
{
    public class Permission : BaseModel
    {
        public string Name { get; set; }

        public string Scope { get; set; }

        public virtual ICollection<ApplicationRole> Roles { get; set; }

        public virtual Module Module { get; set; }

        [ForeignKey("Module")]
        public int? ModuleId { get; set; }
    }
}
