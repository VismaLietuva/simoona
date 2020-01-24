using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Lotteries
{
    public class BuyLotteryTicketViewModel
    {
        [Required]
        public int LotteryId { get; set; }

        [Required]
        public int Tickets { get; set; }
    }
}
