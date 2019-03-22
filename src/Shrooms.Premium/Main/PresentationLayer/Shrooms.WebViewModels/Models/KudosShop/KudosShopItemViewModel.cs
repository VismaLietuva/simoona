using Shrooms.Constants.WebApi;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.Models.KudosShop
{
    public class KudosShopItemViewModel 
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(ConstWebApi.KudosTypeNameMaxLength, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Price { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }
    }
}
