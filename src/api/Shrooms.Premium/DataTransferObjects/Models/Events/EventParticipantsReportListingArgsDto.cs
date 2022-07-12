using Shrooms.Contracts.Infrastructure;
using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventParticipantsReportListingArgsDto : IPageable, ISortableProperties
    {
        public Guid EventId { get; set; }

        public IEnumerable<int> KudosTypeIds { get; set; }

        public IEnumerable<int> EventTypeIds { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public string SortByProperties { get; set; }
    }
}
