using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.WebApi;
using Shrooms.WebViewModels.Models.User;

namespace Shrooms.WebViewModels.Models.ServiceRequests
{
    public class ServiceRequestCategoryCreateViewModel
    {
        [Required]
        [StringLength(ConstWebApi.ServiceRequestCategoryNameMaxLength, MinimumLength = 1)]
        public string Name { get; set; }

        public ICollection<ApplicationUserViewModel> Assignees { get; set; }
    }
}
