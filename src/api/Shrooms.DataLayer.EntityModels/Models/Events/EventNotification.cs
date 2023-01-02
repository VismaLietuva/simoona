using Shrooms.Contracts.DataTransferObjects;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models.Events
{
    public class EventNotification : ISoftDelete
    {
        [Key]
        [ForeignKey(nameof(Event))]
        public Guid EventId { get; set; }

        public Event Event { get; set; }

        [Range(0, short.MaxValue)]
        public int RemindBeforeEventStartInDays { get; set; }

        public bool EventStartNotified { get; set; }
   
        [Range(0, short.MaxValue)]
        public int RemindBeforeEventRegistrationDeadlineInDays { get; set; }

        public bool EventRegistrationDeadlineNotified { get; set; }
    }
}
