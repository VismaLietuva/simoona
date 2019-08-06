using System;
using System.Collections.Generic;

namespace Shrooms.DataTransferObjects.Models.Events
{
    public class EventParticipantsChangeDto
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; }
        public List<string> RemovedUsers { get; set; } = new List<string>();
    }
}

