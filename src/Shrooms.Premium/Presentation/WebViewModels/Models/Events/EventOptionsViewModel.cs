using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Models.Events
{
    public class EventOptionsViewModel
    {
        public int MaxOptions { get; set; }

        public IEnumerable<EventOptionViewModel> Options { get; set; }
    }
}
