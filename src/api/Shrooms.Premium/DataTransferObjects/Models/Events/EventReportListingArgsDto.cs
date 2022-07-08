using Shrooms.Contracts.Infrastructure;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventReportListingArgsDto : IPageable, ISortableProperty
    {
        public string SearchString { get; set; }

        public IEnumerable<int> EventTypeIds { get; set; }

        public IEnumerable<string> OfficeTypeIds { get; set; }

        public string SortByColumnName { get; set; }

        public string SortDirection { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}
