using System;
using System.Collections.Generic;

namespace Shrooms.EntityModels.Models.Events
{
    public class EventOption : BaseModel
    {
        public Guid EventId { get; set; }
        public virtual Event Event { get; set; }
        public string Option { get; set; }
        public virtual ICollection<EventParticipant> EventParticipants { get; set; }
    }
}
