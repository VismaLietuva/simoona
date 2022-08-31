﻿using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels
{
    public class EventJoinRemindEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public EventJoinRemindEmailTemplateViewModel(string userNotificationSettingsUrl)
            : base(userNotificationSettingsUrl)
        {
        }

        public IDictionary<string, string> EventTypes { get; set; } = new Dictionary<string, string>();
    }
}
