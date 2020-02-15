using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Premium.Constants;
using Shrooms.Presentation.WebViewModels.Models;

namespace Shrooms.Premium.Presentation.WebViewModels.ServiceRequests
{
    public class ServiceRequestCategoryCreateViewModel
    {
        [Required]
        [StringLength(ServiceRequestConstants.ServiceRequestCategoryNameMaxLength, MinimumLength = 1)]
        public string Name { get; set; }

        public ICollection<ApplicationUserViewModel> Assignees { get; set; }
    }
}
