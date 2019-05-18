using System;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Events
{
    public class EventJoinResultDTO
    {
        //public int LeftEventId { get; set; }
        public Guid JoinedEventId { get; set; }
        //public string LeftEventName { get; set; }
        public string JoinedEventName { get; set; }
    }
}
