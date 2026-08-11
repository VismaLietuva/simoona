using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Groups
{
    public class GroupMemberPostViewModel
    {
        public int? MembershipId { get; set; }

        [Required]
        public string Id { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
