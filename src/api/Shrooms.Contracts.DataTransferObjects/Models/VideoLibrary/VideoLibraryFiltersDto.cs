using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary
{
    public class VideoLibraryFiltersDto
    {
        public IEnumerable<VideoTypeDto> Types { get; set; }

        public int UncategorisedCount { get; set; }

        public bool HasUncategorised { get; set; }

        public int RecentCount { get; set; }

        public bool HasRecent { get; set; }

        public int TotalCount { get; set; }
    }
}
