using System;
using System.ComponentModel.DataAnnotations;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;

namespace Shrooms.Presentation.WebViewModels.Models.VideoLibrary
{
    public class VideoLibraryItemViewModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Id { get; set; }

        [Required]
        [StringLength(VideoLibraryItem.MaxTitleLength, MinimumLength = 1)]
        public string Title { get; set; }

        [Required]
        [StringLength(VideoLibraryItem.MaxUrlLength, MinimumLength = 1)]
        public string Url { get; set; }

        [StringLength(VideoLibraryItem.MaxDescriptionLength)]
        public string Description { get; set; }

        [StringLength(VideoLibraryItem.MaxPictureIdLength)]
        public string PictureId { get; set; }

        public int? VideoTypeId { get; set; }

        public string VideoTypeTitle { get; set; }

        public DateTime Created { get; set; }
    }
}
