using System.Collections.Generic;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class ServiceRequestCategory : BaseModel
    {
        public string Name { get; set; }

        public virtual ICollection<ApplicationUser> Assignees { get; set; }
    }
}
