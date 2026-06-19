using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventSearchOptionsViewModel : IPageable
    {
        [Required]
        [MinLength(2)]
        public string SearchString { get; set; }

        public EventTimeFrame? View { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = EventsConstants.EventsDefaultPageSize;
    }
}
