using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Events.Reminders
{
    public class EventReminderDetailsDto
    {
        public int RemindBeforeInDays { get; set; }

        public EventRemindType Type { get; set; }

        public int RemindedCount { get; set; }
    }
}
