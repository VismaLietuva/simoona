using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models.Lottery
{
    public class LotteryParticipant : BaseModel
    {
        [ForeignKey("Lottery")]
        public int LotteryId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public DateTime Joined { get; set; }

        public virtual ApplicationUser User { get; set; }

        public Lottery Lottery { get; set; }
    }
}
