using System.Collections.Generic;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class JobPosition : BaseModelWithOrg
    {
        public string Title { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}
