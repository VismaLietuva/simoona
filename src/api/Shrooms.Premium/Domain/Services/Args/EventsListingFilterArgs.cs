using System;

namespace Shrooms.Premium.Domain.Services.Args
{
    public class EventsListingFilterArgs
    {
        public int? TypeId { get; set; }

        public int? OfficeId { get; set; }

        public bool IsOnlyMainEvents { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
     
        public int[] TypeIds { get; set; }

        public string[] OfficeIds { get; set; }
    }
}