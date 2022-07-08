using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventOfficesViewModel
    {
        public string Value { get; set; }

        public IEnumerable<string> OfficeNames { get; set; }
    }
}
