using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class BlacklistState : BaseModelWithOrg
    {
        public string Reason { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
     
        public DateTime EndDate { get; set; }
    }
}
