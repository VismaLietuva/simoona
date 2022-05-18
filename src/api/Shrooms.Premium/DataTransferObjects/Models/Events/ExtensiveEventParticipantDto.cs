using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class ExtensiveEventParticipantDto
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
        
        public IEnumerable<string> PreviouslyAttendedConferences { get; set; }

        public IEnumerable<string> Projects { get; set; }
    }
}
