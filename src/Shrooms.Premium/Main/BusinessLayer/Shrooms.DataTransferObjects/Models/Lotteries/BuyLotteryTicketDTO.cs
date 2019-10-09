using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Lotteries
{
    public class BuyLotteryTicketDTO
    {
        public int LotteryId { get; set; }

        public int Tickets { get; set; }
    }
}
