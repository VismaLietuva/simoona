namespace Shrooms.Premium.DataTransferObjects.Models.Lotteries
{
    public class LotteryTicketGiftedEmailDto
    {
        public LotteryDetailsDto LotteryDetails { get; set; }
        public string BuyerFullName { get; set; }
        public LotteryTicketReceiverDto[] Receivers { get; set; }
    }
}
