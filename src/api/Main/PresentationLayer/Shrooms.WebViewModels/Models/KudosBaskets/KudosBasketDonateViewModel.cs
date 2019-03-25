using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.Models.KudosBaskets
{
    public class KudosBasketDonateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Range(0.01, int.MaxValue)]
        [RegularExpression(@"^\d*.\d{0,2}$", ErrorMessage = "Invalid kudos amount")]
        public decimal DonationAmount { get; set; }
    }
}
