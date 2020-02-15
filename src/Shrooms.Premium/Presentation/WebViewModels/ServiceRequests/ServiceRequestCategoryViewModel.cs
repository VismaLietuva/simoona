using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.ViewModels.User;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Presentation.WebViewModels.ServiceRequests
{
    public class ServiceRequestCategoryViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(ServiceRequestConstants.ServiceRequestCategoryNameMaxLength, MinimumLength = 1)]
        public string Name { get; set; }

        public bool IsNecessary { get; set; }

        public IEnumerable<ApplicationUserMinimalViewModel> Assignees { get; set; }
    }
}
