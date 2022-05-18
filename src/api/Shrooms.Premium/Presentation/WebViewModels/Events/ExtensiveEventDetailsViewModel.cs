using Shrooms.Premium.DataTransferObjects.Models.Events;
using System;
using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class ExtensiveEventDetailsViewModel
    {
        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string[] OfficeNames { get; set; }

        public string HostFirstName { get; set; }

        public string HostLastName { get; set; }

        public string HostUserId { get; set; }

        public string ImageName { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }

        public bool IsForAllOffices { get; set; }

        public IEnumerable<ExtensiveEventParticipantDto> ExtensiveParticipants { get; set; }
    }
}
