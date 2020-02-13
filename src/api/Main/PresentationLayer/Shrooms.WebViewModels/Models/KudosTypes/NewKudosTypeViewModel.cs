using System.ComponentModel.DataAnnotations;
using Shrooms.Host.Contracts.Constants;

namespace Shrooms.WebViewModels.Models.KudosTypes
{
    public class NewKudosTypeViewModel
    {
        [Required]
        [StringLength(WebApiConstants.KudosTypeNameMaxLength, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        public int Multiplier { get; set; }

        [MaxLength(BusinessLayerConstants.MaxKudosDescriptionLength)]
        public string Description { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
