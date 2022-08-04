using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Lotteries
{
    public class EditStartedLotteryViewModel
    {
        [Required]
        public int? Id { get; set; }
        
        public string Description { get; set; }
    }
}
