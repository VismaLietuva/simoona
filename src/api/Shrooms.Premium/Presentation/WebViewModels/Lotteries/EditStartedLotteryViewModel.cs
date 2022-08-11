using Shrooms.Premium.Constants;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Lotteries
{
    public class EditStartedLotteryViewModel
    {
        [Required]
        public int? Id { get; set; }

        [Required]
        [StringLength(ValidationConstants.LotteryDescriptionMaxLength)]
        public string Description { get; set; }
    }
}
