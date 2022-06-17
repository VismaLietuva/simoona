using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventReportListingArgsViewModel : IPageable, ISortableProperty
    {
        [Required]
        [MaxLength(Contracts.Constants.ValidationConstants.MaxCommentMessageBodyLength)]
        public string SearchString { get; set; }

        public IEnumerable<int> EventTypeIds { get; set; }

        public IEnumerable<string> OfficeTypeIds { get; set; }

        public string SortByColumnName { get; set; }

        public string SortDirection { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = EventsConstants.EventsDefaultPageSize;
    }
}
