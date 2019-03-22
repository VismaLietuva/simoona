using Shrooms.Constants.WebApi;
using Shrooms.WebViewModels.Models.User;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.Models.ServiceRequests
{
    public class ServiceRequestCategoryViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(ConstWebApi.ServiceRequestCategoryNameMaxLength, MinimumLength = 1)]
        public string Name { get; set; }

        public bool IsNecessary { get; set; }

        public ICollection<ApplicationUserViewModel> Assignees { get; set; }
    }
}
