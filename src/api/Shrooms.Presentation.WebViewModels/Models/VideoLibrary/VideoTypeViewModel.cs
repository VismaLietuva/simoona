using System.ComponentModel.DataAnnotations;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;

namespace Shrooms.Presentation.WebViewModels.Models.VideoLibrary
{
    public class VideoTypeViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(VideoType.MaxTitleLength, MinimumLength = 1)]
        public string Title { get; set; }

        public int VideoCount { get; set; }
    }
}
