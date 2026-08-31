using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary
{
    public class VideoLibraryFiltersDto
    {
        public IEnumerable<VideoTypeDto> Types { get; set; }

        public int UncategorisedCount { get; set; }

        public int TotalCount { get; set; }
    }
}
