using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class MyEventsOptionsViewModel : IPageable
    {
        public string SearchString { get; set; }

        public MyEventsOptions Filter { get; set; }

        [Required]
        public string OfficeId { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = EventsConstants.EventsDefaultPageSize;
    }
}