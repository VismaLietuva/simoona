using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Infrastructure.Email.Attributes;
using System;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class NewEventEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string EventUrl { get; set; }

        public string EventName { get; set; }

        [ApplyTimeZoneChanges]
        public DateTime StartDate { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public NewEventEmailTemplateViewModel(
            string eventUrl,
            string eventName,
            string description,
            string location,
            DateTime startDate,
            string userNotificationSettingsUrl)
            :
            base(userNotificationSettingsUrl)
        {
            EventUrl = eventUrl;
            EventName = eventName;
            Description = description;
            StartDate = startDate;
            Location = location;
        }
    }
}
