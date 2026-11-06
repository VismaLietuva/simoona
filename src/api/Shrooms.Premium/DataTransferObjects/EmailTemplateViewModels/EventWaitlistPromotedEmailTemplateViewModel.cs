using System;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Infrastructure.Email.Attributes;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class EventWaitlistPromotedEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string EventTitle { get; set; }
        public string EventUrl { get; set; }

        [ApplyTimeZoneChanges]
        public DateTime StartDate { get; set; }

        public EventWaitlistPromotedEmailTemplateViewModel(string userNotificationSettingsUrl, string eventTitle, string eventUrl, DateTime startDate)
            : base(userNotificationSettingsUrl)
        {
            EventTitle = eventTitle;
            EventUrl = eventUrl;
            StartDate = startDate;
        }
    }
}
