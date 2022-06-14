using Shrooms.Contracts.DataTransferObjects.Paging;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventReportListingArgsDto : BasePagingDto
    {
        public string SearchString { get; set; }

        public IEnumerable<int> EventTypeIds { get; set; }

        public IEnumerable<string> OfficeTypeIds { get; set; }
    }
}
