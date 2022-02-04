using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;
using Shrooms.Presentation.WebViewModels.Models.Wall.Moderator;

namespace Shrooms.Presentation.WebViewModels.Models.Wall
{
    public class CreateWallViewModel
    {
        [Required]
        [StringLength(WebApiConstants.WallNameMaxLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(WebApiConstants.WallDescMaxLength)]
        public string Description { get; set; }

        [Required]
        public string Logo { get; set; }

        [Required]
        public IEnumerable<ModeratorViewModel> Moderators { get; set; }

        [Required]
        public bool IsHidden { get; set; }
    }
}
