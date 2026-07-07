using Shrooms.Contracts.Infrastructure;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventSearchOptionsDto : IPageable
    {
        public string SearchString { get; set; }

        public EventTimeFrame View { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}
