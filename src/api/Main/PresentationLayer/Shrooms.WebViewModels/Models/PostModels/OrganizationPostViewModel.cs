using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.BusinessLayer;

namespace Shrooms.WebViewModels.Models.PostModels
{
    public class OrganizationPostViewModel : AbstractViewModel
    {
        [Required]
        [StringLength(ConstBusinessLayer.MaxOrganizationNameLength, MinimumLength = ConstBusinessLayer.MinOrganizationNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(ConstBusinessLayer.MaxOrganizationShortNameLength, MinimumLength = ConstBusinessLayer.MinOrganizationNameLength)]
        public string ShortName { get; set; }
    }
}