using Shrooms.Contracts.ViewModels;
using System;
using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventParticipantsReportListingArgsViewModel : BasePagingViewModel
    {
        public Guid EventId { get; set; }

        public IEnumerable<int> KudosTypeIds { get; set; }

        public IEnumerable<int> EventTypeIds { get; set; }
    }
}
