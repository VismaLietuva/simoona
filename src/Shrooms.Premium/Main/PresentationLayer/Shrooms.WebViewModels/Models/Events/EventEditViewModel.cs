using Shrooms.EntityModels.Models.Events;
using System;
using System.Collections.Generic;

namespace Shrooms.WebViewModels.Models.Events
{
    public class EventEditViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ImageName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime? RegistrationDeadlineDate { get; set; }

        public EventRecurrenceOptions Recurrence { get; set; }
        
        public int? OfficeId { get; set; }

        public string Location { get; set; }

        public int TypeId { get; set; }

        public string Description { get; set; }

        public int MaxParticipants { get; set; }

        public int MaxOptions { get; set; }

        public string HostUserId { get; set; }

        public string HostUserFullName { get; set; }

        public bool ResetParticipantList { get; set; }

        public IEnumerable<EventOptionViewModel> Options { get; set; }
    }
}
