using System;
using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventDetailsListItemViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string TypeName { get; set; }

        public IEnumerable<string> OfficeNames { get; set; }

        public int MaxVirtualParticipants { get; set; }

        public int MaxParticipants { get; set; }

        public int GoingCount { get; set; }

        public int VirtuallyGoingCount { get; set; }

        public bool IsForAllOffices { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
