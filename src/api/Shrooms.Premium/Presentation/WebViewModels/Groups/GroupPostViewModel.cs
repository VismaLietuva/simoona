using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.ViewModels.User;

namespace Shrooms.Premium.Presentation.WebViewModels.Groups
{
    public class GroupPostViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Markdown. Rendered through the shared markdown directive on the client.
        /// </summary>
        [StringLength(5000)]
        public string Description { get; set; }

        public string PictureId { get; set; }

        [Required]
        public int GroupTypeId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(10000)]
        public string ApprovalAnswers { get; set; }

        public ICollection<GroupMemberPostViewModel> Members { get; set; }

        public ICollection<GroupReferenceViewModel> References { get; set; }
    }
}
