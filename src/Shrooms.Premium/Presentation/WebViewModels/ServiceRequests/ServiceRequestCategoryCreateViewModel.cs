using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.ViewModels.User;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Presentation.WebViewModels.ServiceRequests
{
    public class ServiceRequestCategoryCreateViewModel
    {
        [Required]
        [StringLength(ServiceRequestConstants.ServiceRequestCategoryNameMaxLength, MinimumLength = 1)]
        public string Name { get; set; }

        public ICollection<ApplicationUserMinimalViewModel> Assignees { get; set; }
    }
}
