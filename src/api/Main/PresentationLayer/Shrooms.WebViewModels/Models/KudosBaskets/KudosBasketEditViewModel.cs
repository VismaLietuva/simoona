using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.WebApi;

namespace Shrooms.WebViewModels.Models.KudosBaskets
{
    public class KudosBasketEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(KudosBasketConstants.KudosBasketMaxTitleLength)]
        public string Title { get; set; }

        [MaxLength(KudosBasketConstants.KudosBasketMaxDescriptionLength)]
        public string Description { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
