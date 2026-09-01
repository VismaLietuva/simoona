using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models.Polls
{
    public class PollAnswer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        public int PollId { get; set; }

        [ForeignKey(nameof(PollId))]
        public virtual Poll Poll { get; set; }

        public int PollQuestionId { get; set; }

        [ForeignKey(nameof(PollQuestionId))]
        public virtual PollQuestion Question { get; set; }

        public int PollOptionId { get; set; }

        [ForeignKey(nameof(PollOptionId))]
        public virtual PollOption Option { get; set; }

        public string ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
