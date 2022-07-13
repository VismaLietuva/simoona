using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventReportListingArgsViewModel : IPageable, ISortable
    {
        [MaxLength(Contracts.Constants.ValidationConstants.MaxCommentMessageBodyLength)]
        public string SearchString { get; set; }

        public IEnumerable<int> EventTypeIds { get; set; }

        public IEnumerable<string> OfficeTypeIds { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = EventsConstants.EventsDefaultPageSize;

        public string SortByProperties { get; set; }
    }
}
