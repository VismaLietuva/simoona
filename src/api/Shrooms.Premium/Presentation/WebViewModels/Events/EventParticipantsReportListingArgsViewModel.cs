using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;
using System;
using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventParticipantsReportListingArgsViewModel : IPageable, ISortableProperty
    {
        public Guid EventId { get; set; }

        public IEnumerable<int> KudosTypeIds { get; set; }

        public IEnumerable<int> EventTypeIds { get; set; }

        public string SortByColumnName { get; set; }

        public string SortDirection { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = EventsConstants.EventsDefaultPageSize;
    }
}
