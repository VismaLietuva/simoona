using Shrooms.Contracts.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class BlacklistUser : BaseModelWithOrg
    {
        public string Reason { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public ApplicationUser ModifiedByUser { get; set; }

        public ApplicationUser CreatedByUser { get; set; }

        public DateTime EndDate { get; set; }

        public BlacklistStatus Status { get; set; }
    }
}
