using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Lotteries
{
    public class LotteryTicketReceiverViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int? TicketCount { get; set; }
    }
}
