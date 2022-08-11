namespace Shrooms.Premium.DataTransferObjects.Models.Lotteries
{
    public class BuyLotteryTicketsDto
    {
        public int LotteryId { get; set; }

        public int TicketCount { get; set; }

        public LotteryTicketReceiverDto[] Receivers { get; set; }
    }
}
