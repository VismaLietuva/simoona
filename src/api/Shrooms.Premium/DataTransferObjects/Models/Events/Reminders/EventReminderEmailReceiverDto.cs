﻿using Shrooms.Contracts.Infrastructure.Email;

namespace Shrooms.Premium.DataTransferObjects.Models.Events.Reminders
{
    public class EventReminderEmailReceiverDto : IEmailReceiver
    {
        public string Email { get; set; }

        public string TimeZoneKey { get; set; }
    }
}
