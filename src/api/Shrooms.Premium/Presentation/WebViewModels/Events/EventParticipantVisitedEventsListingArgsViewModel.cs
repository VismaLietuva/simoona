using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;
using Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventParticipantVisitedEventsListingArgsViewModel : IPageable, ISortable, IFilterableByDate
    {
        public string UserId { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = EventsConstants.EventsDefaultPageSize;
        
        public string SortByProperties { get; set; }

        public IEnumerable<int> EventTypeIds { get; set; }

        [Required]
        [DateTimeLessThanDateTime(nameof(EndDate))]
        public DateTime? StartDate { get; set; }
        
        [Required]
        [DateTimeLessThanPresentDate]
        public DateTime? EndDate { get; set; }
    }
}
