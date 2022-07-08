using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventReportDetailsDto
    {
        public string Name { get; set; }

        public string ImageName { get; set; }

        public EventOfficesDto Offices { get; set; }

        public IEnumerable<string> OfficeNames { get; set; }

        public string Location { get; set; }

        public bool IsForAllOffices { get; set; }
        
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string HostUserFullName { get; set; }

        public string HostUserId { get; set; }
    }
}
