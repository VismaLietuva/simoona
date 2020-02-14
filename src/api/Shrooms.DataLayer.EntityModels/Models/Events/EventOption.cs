using System;
using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Events
{
    public class EventOption : BaseModel
    {
        public Guid EventId { get; set; }
        public virtual Event Event { get; set; }
        public string Option { get; set; }
        public OptionRules Rule { get; set; }
        public virtual ICollection<EventParticipant> EventParticipants { get; set; }
    }
}
