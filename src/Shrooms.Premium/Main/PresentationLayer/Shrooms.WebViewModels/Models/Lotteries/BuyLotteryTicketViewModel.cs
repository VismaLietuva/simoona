using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Premium.Main.PresentationLayer.Shrooms.WebViewModels.Models.Lotteries
{
    public class BuyLotteryTicketViewModel
    {
        [Required]
        public int LotteryId { get; set; }

        [Required]
        public int Tickets { get; set; }
    }
}
