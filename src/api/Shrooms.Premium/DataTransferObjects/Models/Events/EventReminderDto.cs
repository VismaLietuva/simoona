using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventReminderDto
    {
        public int RemindBeforeInDays { get; set; }

        public EventRemindType Type { get; set; }
    }
}
