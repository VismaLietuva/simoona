namespace Shrooms.Premium.DataTransferObjects.Models.Lotteries
{
    public class BuyLotteryTicketsDto
    {
        public int LotteryId { get; set; }

        public int Tickets { get; set; }

        public string[] ReceivingUserIds { get; set; }
    }
}
