﻿using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventReminderViewModel
    {
        public int RemindBeforeInDays { get; set; }

        public EventReminderType Type { get; set; }
    }
}
