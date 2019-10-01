using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.BusinessLayer;
using Shrooms.Constants.WebApi;

namespace Shrooms.WebViewModels.Models.Kudos
{
    public class KudosTypeViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(ConstWebApi.KudosTypeNameMaxLength, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        public decimal Value { get; set; }

        public ConstBusinessLayer.KudosTypeEnum Type { get; set; }

        public bool Hidden { get; set; }

        public bool IsNecessary { get; set; }

        [MaxLength(ConstBusinessLayer.MaxKudosDescriptionLength)]
        public string Description { get; set; }
    }
}