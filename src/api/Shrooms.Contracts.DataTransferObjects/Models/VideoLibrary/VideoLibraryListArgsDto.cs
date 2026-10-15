using System;
using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary
{
    public class VideoLibraryListArgsDto : UserAndOrganizationDto, IPageable
    {
        public string Search { get; set; }

        public int? VideoTypeId { get; set; }

        public bool Uncategorised { get; set; }

        public bool Recent { get; set; }

        public DateOnly? From { get; set; }

        public DateOnly? To { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}
