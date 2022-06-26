using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventFilteredArgsViewModel : IPageable
    {
        public string TypeId { get; set; } = null;

        public string OfficeId { get; set; } = null;

        public DateTime? StartDate { get; set; } = null;

        public DateTime? EndDate { get; set; } = null;

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = EventsConstants.EventsDefaultPageSize;
    }
}
