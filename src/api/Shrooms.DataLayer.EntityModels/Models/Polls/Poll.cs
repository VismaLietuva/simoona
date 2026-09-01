using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.EntityModels.Models.Polls
{
    public class Poll : SoftDeletableModelWithOrg
    {
        public const int MaxTitleLength = 100;
        public const int MaxDescriptionLength = 300;
        public const int MaxReasonLength = 500;

        [Required]
        [StringLength(MaxTitleLength)]
        public string Title { get; set; }

        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        public bool IsAnonymous { get; set; }

        public bool IsOfficial { get; set; }

        public DateTime Deadline { get; set; }

        public DateTime? ClosedAt { get; set; }

        public PollState State { get; set; }

        public int WallId { get; set; }

        [ForeignKey(nameof(WallId))]
        public virtual Wall Wall { get; set; }

        public string ReviewedById { get; set; }

        [ForeignKey(nameof(ReviewedById))]
        public virtual ApplicationUser ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [StringLength(MaxReasonLength)]
        public string ReviewReason { get; set; }

        public virtual ICollection<PollQuestion> Questions { get; set; }
    }
}
