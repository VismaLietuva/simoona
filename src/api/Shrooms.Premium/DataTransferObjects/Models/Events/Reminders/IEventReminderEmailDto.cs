using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events.Reminders
{
    public interface IEventReminderEmailDto
    {
        IEnumerable<EventReminderEmailReceiverDto> Receivers { get; set; }

        Guid EventId { get; set; }

        string EventName { get; set; }
    }
}