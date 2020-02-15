using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Kudos
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
