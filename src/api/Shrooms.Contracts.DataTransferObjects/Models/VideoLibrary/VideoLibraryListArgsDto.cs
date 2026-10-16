using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary
{
    public class VideoLibraryListArgsDto : VideoLibraryFiltersArgsDto, IPageable
    {
        public int? VideoTypeId { get; set; }

        public bool Uncategorised { get; set; }

        public bool Recent { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}
