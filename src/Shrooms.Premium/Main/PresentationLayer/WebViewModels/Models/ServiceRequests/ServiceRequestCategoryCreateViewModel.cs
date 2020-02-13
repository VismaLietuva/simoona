using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Premium.Constants;
using Shrooms.WebViewModels.Models.User;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.ServiceRequests
{
    public class ServiceRequestCategoryCreateViewModel
    {
        [Required]
        [StringLength(ServiceRequestConstants.ServiceRequestCategoryNameMaxLength, MinimumLength = 1)]
        public string Name { get; set; }

        public ICollection<ApplicationUserViewModel> Assignees { get; set; }
    }
}
