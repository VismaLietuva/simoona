using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models.Polls
{
    public class PollQuestion : SoftDeletableModel
    {
        public const int MaxTextLength = 200;

        public int PollId { get; set; }

        [ForeignKey(nameof(PollId))]
        public virtual Poll Poll { get; set; }

        [Required]
        [StringLength(MaxTextLength)]
        public string Text { get; set; }

        public bool AllowMultiple { get; set; }

        public int Order { get; set; }

        public virtual ICollection<PollOption> Options { get; set; }
    }
}
