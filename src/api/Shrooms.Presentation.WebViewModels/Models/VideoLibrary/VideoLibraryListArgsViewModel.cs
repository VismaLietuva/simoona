using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;

namespace Shrooms.Presentation.WebViewModels.Models.VideoLibrary
{
    public class VideoLibraryListArgsViewModel : IPageable
    {
        public const int DefaultPageSize = 24;
        public const int MaxPageSize = 100;

        [StringLength(VideoLibraryItem.MaxTitleLength)]
        public string Search { get; set; }

        [Range(1, int.MaxValue)]
        public int? VideoTypeId { get; set; }

        public bool Uncategorised { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, MaxPageSize)]
        public int PageSize { get; set; } = DefaultPageSize;
    }
}
