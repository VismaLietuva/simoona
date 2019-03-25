using System.Collections.Generic;

namespace Shrooms.EntityModels.Models
{
    public class Module : BaseModel
    {
        public string Name { get; set; }
        public virtual ICollection<Organization> Organizations { get; set; }
    }
}
