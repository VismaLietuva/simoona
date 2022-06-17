using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class MyEventsOptionsViewModel : IPageable
    {
        public string SearchString { get; set; }

        public MyEventsOptions Filter { get; set; }

        public string OfficeId { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = EventsConstants.EventsDefaultPageSize;
    }
}