using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.WebApi;
using Shrooms.WebViewModels.Models.Wall.Moderator;

namespace Shrooms.WebViewModels.Models.Wall
{
    public class CreateWallViewModel
    {
        [Required]
        [StringLength(ConstWebApi.WallNameMaxLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(ConstWebApi.WallDescMaxLength)]
        public string Description { get; set; }

        [Required]
        public string Logo { get; set; }

        [Required]
        public IEnumerable<ModeratorViewModel> Moderators { get; set; }
    }
}
