using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.BusinessLayer;

namespace Shrooms.EntityModels.Models.Kudos
{
    public class KudosType : BaseModel
    {
        [Required]
        public string Name { get; set; }

        public decimal Value { get; set; }

        public ConstBusinessLayer.KudosTypeEnum Type { get; set; }
        
        [MaxLength(ConstBusinessLayer.MaxKudosDescriptionLength)]
        public string Description { get; set; }
    }
}
