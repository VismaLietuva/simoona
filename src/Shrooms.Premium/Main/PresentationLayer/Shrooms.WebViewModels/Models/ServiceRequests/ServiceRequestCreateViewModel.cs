using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.EntityValidationValues;

namespace Shrooms.WebViewModels.Models.ServiceRequests
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
    }
}
