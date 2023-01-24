using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.DataLayer.EntityModels.Models.Events
{
    public class EventReminder : BaseModel
    {
        public Guid EventId { get; set; }

        public Event Event { get; set; }
        
        [Range(ValidationConstants.EventReminderRemindBeforeInDaysMin, ValidationConstants.EventReminderRemindBeforeInDaysMax)]
        public int RemindBeforeInDays { get; set; }

        public EventRemindType Type { get; set; }
        
        public bool Reminded { get; set; }
    }
}
