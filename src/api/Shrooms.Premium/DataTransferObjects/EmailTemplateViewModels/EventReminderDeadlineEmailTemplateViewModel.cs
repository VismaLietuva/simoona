using Shrooms.Contracts.DataTransferObjects;
using System;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class EventReminderDeadlineEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime DeadlineDate { get; set; }

        public EventReminderDeadlineEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            string name,
            string url,
            DateTime startDate,
            DateTime deadlineDate) : base(userNotificationSettingsUrl)
        {
            Name = name;
            Url = url;
            StartDate = startDate;
            DeadlineDate = deadlineDate;
        }
    }
}
