using System;

namespace Shrooms.Domain.Services.Args
{
    public class EventsListingFilterArgs
    {
        public int? TypeId { get; set; }

        public int? OfficeId { get; set; }

        public bool IsOnlyMainEvents { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}