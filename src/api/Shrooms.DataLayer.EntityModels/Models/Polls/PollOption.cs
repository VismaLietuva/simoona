using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models.Polls
{
    public class PollOption : SoftDeletableModel
    {
        public const int MaxTextLength = 100;
        public const int MaxUrlLength = 2000;

        public int PollQuestionId { get; set; }

        [ForeignKey(nameof(PollQuestionId))]
        public virtual PollQuestion Question { get; set; }

        [Required]
        [StringLength(MaxTextLength)]
        public string Text { get; set; }

        [StringLength(MaxUrlLength)]
        public string Url { get; set; }

        public int Order { get; set; }
    }
}
