using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Lotteries
{
    public class BuyLotteryTicketsViewModel
    {
        [Required]
        public int? LotteryId { get; set; }

        [Required]
        public int? Tickets { get; set; }

        [MinLength(1)]
        public string[] ReceivingUserIds { get; set; }
    }
}
