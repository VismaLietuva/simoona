using System;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Users;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class CoacheeJoinedEventEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string CoacheeFullName { get; set; }

        public string EventName { get; set; }

        public string EventUrl { get; set; }

        public DateTime EventStartDate { get; set; }

        public DateTime EventEndDate { get; set; }

        public CoacheeJoinedEventEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            UserEventAttendStatusChangeEmailDto eventAttendStatusDto,
            string eventUrl)
            : base(userNotificationSettingsUrl)
        {
            CoacheeFullName = eventAttendStatusDto.FullName;
            EventName = eventAttendStatusDto.EventName;
            EventStartDate = eventAttendStatusDto.EventStartDate;
            EventEndDate = eventAttendStatusDto.EventEndDate;
            EventUrl = eventUrl;
        }
    }
}
