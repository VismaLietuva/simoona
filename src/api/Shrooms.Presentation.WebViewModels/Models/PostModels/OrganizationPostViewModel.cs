using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.ViewModels;

namespace Shrooms.Presentation.WebViewModels.Models.PostModels
{
    public class OrganizationPostViewModel : AbstractViewModel
    {
        [Required]
        [StringLength(BusinessLayerConstants.MaxOrganizationNameLength, MinimumLength = BusinessLayerConstants.MinOrganizationNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(BusinessLayerConstants.MaxOrganizationShortNameLength, MinimumLength = BusinessLayerConstants.MinOrganizationNameLength)]
        public string ShortName { get; set; }
    }
}