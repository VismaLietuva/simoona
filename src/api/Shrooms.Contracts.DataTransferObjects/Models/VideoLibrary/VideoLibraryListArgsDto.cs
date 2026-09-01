using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary
{
    public class VideoLibraryListArgsDto : UserAndOrganizationDto, IPageable
    {
        public string Search { get; set; }

        public int? VideoTypeId { get; set; }

        public bool Uncategorised { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}
