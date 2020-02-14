using System.ComponentModel.DataAnnotations;
using Shrooms.Premium.Constants;
using Shrooms.Presentation.WebViewModels.Models;

namespace Shrooms.Premium.Presentation.WebViewModels.Models.ServiceRequests
{
    public class ServiceRequestPostViewModel : AbstractViewModel
    {
        [MaxLength(ValidationConstants.ServiceRequestMaxTitleLength, ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "MaxLengthError")]
        public string Title { get; set; }

        public int PriorityId { get; set; }

        [MaxLength(ValidationConstants.ServiceRequestMaxDescriptionLength, ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "MaxLengthError")]
        public string Description { get; set; }

        public int ServiceRequestCategoryId { get; set; }

        public int StatusId { get; set; }

        [Range(ValidationConstants.KudosMultiplyByMinValue, ValidationConstants.KudosMultiplyByMaxValue)]
        public int? KudosAmmount { get; set; }
    }
}