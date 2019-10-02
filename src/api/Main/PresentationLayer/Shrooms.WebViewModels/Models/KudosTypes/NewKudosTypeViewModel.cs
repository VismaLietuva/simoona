using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.BusinessLayer;
using Shrooms.Constants.WebApi;

namespace Shrooms.WebViewModels.Models.KudosTypes
{
    public class NewKudosTypeViewModel
    {
        [Required]
        [StringLength(ConstWebApi.KudosTypeNameMaxLength, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        public int Multiplier { get; set; }

        [MaxLength(ConstBusinessLayer.MaxKudosDescriptionLength)]
        public string Description { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
