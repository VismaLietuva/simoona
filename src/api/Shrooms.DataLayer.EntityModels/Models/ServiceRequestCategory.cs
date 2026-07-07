using System.Collections.Generic;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class ServiceRequestCategory : SoftDeletableModel
    {
        public string Name { get; set; }

        public virtual ICollection<ApplicationUser> Assignees { get; set; }
    }
}
