using System.ComponentModel.DataAnnotations;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Presentation.WebViewModels.ServiceRequests
{
    public class ServiceRequestCreateViewModel
    {
        [Required]
        [MaxLength(ValidationConstants.ServiceRequestMaxTitleLength)]
        public string Title { get; set; }

        [Required]
        public int PriorityId { get; set; }

        [MaxLength(ValidationConstants.ServiceRequestMaxDescriptionLength)]
        public string Description { get; set; }

        [Required]
        public int ServiceRequestCategoryId { get; set; }

        [Range(ValidationConstants.KudosMultiplyByMinValue, ValidationConstants.KudosMultiplyByMaxValue)]
        public int? KudosAmmount { get; set; }

        public int? KudosShopItemId { get; set; }

        public string PictureId { get; set; }

        /// <summary>
        /// Gets or sets whether the kudos deduction's comment is hidden from other users. Null leaves the stored value unchanged.
        /// </summary>
        public bool? IsKudosCommentHidden { get; set; }
    }
}
