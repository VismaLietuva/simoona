using Shrooms.Contracts.Infrastructure;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventReportListingArgsDto : IPageable, ISortable
    {
        public string SearchString { get; set; }

        public IEnumerable<int> EventTypeIds { get; set; }

        public IEnumerable<string> OfficeTypeIds { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public string SortByProperties { get; set; }
    }
}
