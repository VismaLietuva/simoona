using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events.Reminders
{
    public class RemindEventDeadlineEmailDto : IRemindEventDto
    {
        public DateTime StartDate { get; set; }

        public DateTime DeadlineDate { get; set; }

        public IEnumerable<RemindReceiverDto> Receivers { get; set; }

        public Guid EventId { get; set; }

        public string EventName { get; set; }
    }
}