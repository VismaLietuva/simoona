using System.Collections.Generic;

namespace Shrooms.Presentation.WebViewModels.Models.Wall.Mentions
{
    public class MentionSuggestionsViewModel
    {
        public IEnumerable<MentionPersonViewModel> People { get; set; }

        public IEnumerable<MentionGroupViewModel> Groups { get; set; }
    }
}
