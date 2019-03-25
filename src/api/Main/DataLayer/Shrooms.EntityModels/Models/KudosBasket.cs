using System.Collections.Generic;
using Shrooms.EntityModels.Models.Kudos;

namespace Shrooms.EntityModels.Models
{
    public class KudosBasket : BaseModelWithOrg
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<KudosLog> KudosLogs { get; set; }
    }
}
