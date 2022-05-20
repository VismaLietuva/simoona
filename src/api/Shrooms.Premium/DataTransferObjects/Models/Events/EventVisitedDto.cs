using System;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventVisitedDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string TypeName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
