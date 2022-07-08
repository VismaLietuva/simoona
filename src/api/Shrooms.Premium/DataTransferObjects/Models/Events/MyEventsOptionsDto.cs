using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class MyEventsOptionsDto : IPageable
    {
        public string SearchString { get; set; }

        public MyEventsOptions Filter { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}
