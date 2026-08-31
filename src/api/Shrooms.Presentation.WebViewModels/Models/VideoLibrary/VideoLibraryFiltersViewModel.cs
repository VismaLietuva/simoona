using System.Collections.Generic;

namespace Shrooms.Presentation.WebViewModels.Models.VideoLibrary
{
    public class VideoLibraryFiltersViewModel
    {
        public IEnumerable<VideoTypeViewModel> Types { get; set; }

        public int UncategorisedCount { get; set; }

        public int TotalCount { get; set; }
    }
}
