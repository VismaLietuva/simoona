using System;
using Shrooms.Contracts.Infrastructure;
using System.Collections.Generic;
using Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventReportListingArgsDto : IPageable, ISortable, IFilterableByDate
    {
        public string SearchString { get; set; }

        public IEnumerable<int> EventTypeIds { get; set; }

        public IEnumerable<string> OfficeTypeIds { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public string SortByProperties { get; set; }

        [DateTimeLessThanDateTime(nameof(EndDate))]
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }

        public bool ExcludeEmptyEvents { get; set; }
    }
}
