using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Events.Reminders
{
    public class EventReminderDto
    {
        public int RemindBeforeInDays { get; set; }

        public EventReminderType Type { get; set; }
    }
}
