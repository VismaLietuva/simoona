using Shrooms.Contracts.DataTransferObjects;
using System;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class EventReminderStartEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public DateTime StartDate { get; set; }

        public EventReminderStartEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            string name,
            string url,
            DateTime startDate) : base(userNotificationSettingsUrl)
        {
            Name = name;
            Url = url;
            StartDate = startDate;
        }
    }
}
