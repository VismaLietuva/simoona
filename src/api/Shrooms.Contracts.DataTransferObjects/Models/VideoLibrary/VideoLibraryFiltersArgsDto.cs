using System;

namespace Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary
{
    public class VideoLibraryFiltersArgsDto : UserAndOrganizationDto
    {
        public string Search { get; set; }

        public DateOnly? From { get; set; }

        public DateOnly? To { get; set; }
    }
}
