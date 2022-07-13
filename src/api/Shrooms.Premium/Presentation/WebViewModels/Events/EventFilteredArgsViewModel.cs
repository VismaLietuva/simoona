using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;
using Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventFilteredArgsViewModel : IPageable, IFilterableByDate
    {
        public string TypeId { get; set; }

        public string OfficeId { get; set; }

        [DateTimeLessThanDateTime("EndDate")]
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = EventsConstants.EventsDefaultPageSize;
    }
}
