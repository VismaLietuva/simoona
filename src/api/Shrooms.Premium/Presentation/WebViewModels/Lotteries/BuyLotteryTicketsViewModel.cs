using Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Lotteries
{
    public class BuyLotteryTicketsViewModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int? LotteryId { get; set; }

        public int TicketCount { get; set; } // Can be negative

        [MinLength(1)]
        [NoDuplicatesInCollection(nameof(LotteryTicketReceiverViewModel.UserId))]
        public LotteryTicketReceiverViewModel[] Receivers { get; set; }
    }
}
