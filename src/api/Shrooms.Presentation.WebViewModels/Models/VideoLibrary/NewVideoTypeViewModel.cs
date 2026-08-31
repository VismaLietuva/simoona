using System.ComponentModel.DataAnnotations;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;

namespace Shrooms.Presentation.WebViewModels.Models.VideoLibrary
{
    public class NewVideoTypeViewModel
    {
        [Required]
        [StringLength(VideoType.MaxTitleLength, MinimumLength = 1)]
        public string Title { get; set; }
    }
}
