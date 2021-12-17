using System;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventJoinResultDto
    {
        //public int LeftEventId { get; set; }
        public Guid JoinedEventId { get; set; }
        //public string LeftEventName { get; set; }
        public string JoinedEventName { get; set; }
    }
}
