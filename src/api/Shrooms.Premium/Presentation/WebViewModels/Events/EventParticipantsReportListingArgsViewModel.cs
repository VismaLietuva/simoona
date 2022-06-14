using Shrooms.Contracts.ViewModels;
using System;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventParticipantsReportListingArgsViewModel : BasePagingViewModel
    {
        public Guid EventId { get; set; }

        public int[] KudosTypeIds { get; set; }

        public int[] EventTypeIds { get; set; }
    }
}
