using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;

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