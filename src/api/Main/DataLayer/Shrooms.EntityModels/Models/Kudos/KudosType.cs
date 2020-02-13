using System.ComponentModel.DataAnnotations;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.Enums;

namespace Shrooms.EntityModels.Models.Kudos
{
    public class KudosType : BaseModel
    {
        [Required]
        public string Name { get; set; }

        public decimal Value { get; set; }

        public KudosTypeEnum Type { get; set; }

        [MaxLength(BusinessLayerConstants.MaxKudosDescriptionLength)]
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
