using System;
using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventParticipantReportViewModel
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? EmploymentDate { get; set; }

        public string QualificationLevel { get; set; }

        public string JobTitle { get; set; }

        public string ManagerFirstName { get; set; }

        public string ManagerLastName { get; set; }

        public string ManagerId { get; set; }

        public decimal? Kudos { get; set; }

        public ICollection<string> PreviouslyAttendedConferences { get; set; }

        public ICollection<EventVisitedReportViewModel> VisitedEvents { get; set; }

        public ICollection<EventProjectReportViewModel> Projects { get; set; }
    }
}
