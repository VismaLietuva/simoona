using System;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Calendar
{
    public class CalendarEventDTO
    {
        public Guid EventId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Location { get; set; }
    }
}
