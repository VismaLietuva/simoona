using Shrooms.Contracts.DataTransferObjects.Paging;
using System;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventParticipantsReportListingArgsDto : BasePagingDto
    {
        public Guid EventId { get; set; }

        public int[] KudosTypeIds { get; set; }

        public int[] EventTypeIds { get; set; }
    }
}
