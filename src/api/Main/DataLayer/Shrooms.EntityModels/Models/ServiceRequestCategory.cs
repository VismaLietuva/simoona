using System.Collections.Generic;

namespace Shrooms.EntityModels.Models
{
    public class ServiceRequestCategory : BaseModel
    {
        public string Name { get; set; }

        public virtual ICollection<ApplicationUser> Assignees { get; set; }
    }
}
