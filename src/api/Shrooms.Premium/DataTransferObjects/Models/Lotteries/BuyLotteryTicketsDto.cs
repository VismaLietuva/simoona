using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Lotteries
{
    public class BuyLotteryTicketsDto
    {
        public int LotteryId { get; set; }

        public int TicketCount { get; set; }

        public IEnumerable<LotteryTicketReceiverDto> Receivers { get; set; }
    }
}
