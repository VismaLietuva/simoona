using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventReminderDetailsViewModel
    {
        public int RemindBeforeInDays { get; set; }

        public EventRemindType Type { get; set; }

        public bool IsDisabled { get; set; }
    }
}
