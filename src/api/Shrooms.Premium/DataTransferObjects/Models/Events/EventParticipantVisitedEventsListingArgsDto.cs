using Shrooms.Contracts.Infrastructure;
using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventParticipantVisitedEventsListingArgsDto : IPageable, ISortable, IFilterableByDate
    {
        public string UserId { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public string SortByProperties { get; set; }

        public IEnumerable<int> EventTypeIds { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
