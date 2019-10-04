using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.EntityModels.Models.Lotteries
{
    public class LotteryParticipant : BaseModel
    {
        [ForeignKey("Lottery")]
        public int LotteryId { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public DateTime Entered { get; set; }


        public virtual ApplicationUser User { get; set; }
        public Lottery Lottery { get; set; }
    }
}
