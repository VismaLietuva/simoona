using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events.Reminders
{
    public interface IRemindEventDto
    {
        public IEnumerable<RemindReceiverDto> Receivers { get; set; }

        public Guid EventId { get; set; }

        public string EventName { get; set; }
    }
}