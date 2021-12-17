using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.ViewModels;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Presentation.WebViewModels.ServiceRequests
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