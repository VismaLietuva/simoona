using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.DataLayer.EntityModels.Models.Events
{
    public class EventReminder : ISoftDelete
    {
        public int Id { get; set; }

        public Guid EventId { get; set; }

        public Event Event { get; set; }
        
        [Range(ValidationConstants.EventReminderRemindBeforeInDaysMin, ValidationConstants.EventReminderRemindBeforeInDaysMax)]
        public int RemindBeforeInDays { get; set; }

        public EventRemindType Type { get; set; }
        
        public bool Reminded { get; set; }
    }
}
