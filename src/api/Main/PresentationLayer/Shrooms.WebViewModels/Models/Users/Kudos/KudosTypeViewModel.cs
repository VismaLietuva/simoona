using System.ComponentModel.DataAnnotations;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.Enums;

namespace Shrooms.WebViewModels.Models.Users.Kudos
{
    public class KudosTypeViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(WebApiConstants.KudosTypeNameMaxLength, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        public decimal Value { get; set; }

        public KudosTypeEnum Type { get; set; }

        public bool Hidden { get; set; }

        public bool IsNecessary { get; set; }

        [MaxLength(BusinessLayerConstants.MaxKudosDescriptionLength)]
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}