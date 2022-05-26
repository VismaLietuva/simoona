using System;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventVisitedViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string TypeName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
