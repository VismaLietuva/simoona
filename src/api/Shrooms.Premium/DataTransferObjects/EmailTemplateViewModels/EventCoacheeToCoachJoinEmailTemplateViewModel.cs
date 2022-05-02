using System;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class EventCoacheeToCoachJoinEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string CoacheeFullName { get; set; }

        public string EventName { get; set; }

        public string EventUrl { get; set; }

        public DateTime EventStartDate { get; set; }

        public DateTime EventEndDate { get; set; }

        public EventCoacheeToCoachJoinEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            string coacheeFullName,
            string eventName,
            DateTime eventStartDate,
            DateTime eventEndDate,
            string eventUrl)
            : base(userNotificationSettingsUrl)
        {
            CoacheeFullName = coacheeFullName;
            EventName = eventName;
            EventStartDate = eventStartDate;
            EventEndDate = eventEndDate;
            EventUrl = eventUrl;
        }
    }
}
