using Shrooms.Contracts.Infrastructure;
using System;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventFilteredArgsDto : IPageable, IFilterableByDate
    {
        public string TypeId { get; set; }

        public string OfficeId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? TypeIdParsed { get; set; }

        public int? OfficeIdParsed { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}
