using System.Collections.Generic;

namespace Shrooms.Presentation.WebViewModels.Models.VideoLibrary
{
    public class VideoLibraryFiltersViewModel
    {
        public IEnumerable<VideoTypeViewModel> Types { get; set; }

        public int UncategorisedCount { get; set; }

        public bool HasUncategorised { get; set; }

        public int RecentCount { get; set; }

        public bool HasRecent { get; set; }

        public int TotalCount { get; set; }
    }
}
