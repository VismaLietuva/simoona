using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.EntityModels.Models.Events
{
    public class EventParticipant : BaseModel
    {
        public Guid EventId { get; set; }
        public virtual Event Event { get; set; }
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<EventOption> EventOptions { get; set; }
    }
}
