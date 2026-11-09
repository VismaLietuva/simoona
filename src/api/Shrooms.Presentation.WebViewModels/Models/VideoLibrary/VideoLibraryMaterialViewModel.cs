using System.ComponentModel.DataAnnotations;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;

namespace Shrooms.Presentation.WebViewModels.Models.VideoLibrary
{
    public class VideoLibraryMaterialViewModel
    {
        [Required]
        [StringLength(VideoLibraryMaterial.MaxTitleLength, MinimumLength = 1)]
        public string Title { get; set; }

        [Required]
        [StringLength(VideoLibraryMaterial.MaxUrlLength, MinimumLength = 1)]
        public string Url { get; set; }
    }
}
