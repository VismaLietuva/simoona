using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Lotteries
{
    public class BuyLotteryTicketViewModel
    {
        [Required]
        public int LotteryId { get; set; }

        [Required]
        public int Tickets { get; set; }
    }
}
