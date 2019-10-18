using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.Models.Lotteries
{
    public class LotteryStatsDTO
    {
        public int TotalParticipants { get; set; }
        public int TicketsSold { get; set; }
        public int KudosSpent { get; set; }
        public IEnumerable<LotteryParticipantDTO> Participants { get; set; }
    }
}
