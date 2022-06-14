using Shrooms.Contracts.Constants;
using Shrooms.Contracts.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventReportListingArgsViewModel : BasePagingViewModel
    {
        [Required]
        [MaxLength(ValidationConstants.MaxCommentMessageBodyLength)]
        public string SearchString { get; set; }

        public IEnumerable<int> EventTypeIds { get; set; }

        public IEnumerable<string> OfficeTypeIds { get; set; }
    }
}
