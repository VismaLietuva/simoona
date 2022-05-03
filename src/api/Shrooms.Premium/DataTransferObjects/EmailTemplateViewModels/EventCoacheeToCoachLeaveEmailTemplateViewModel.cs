using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Users;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class EventCoacheeToCoachLeaveEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string CoacheeFullName { get; set; }

        public string EventName { get; set; }

        public string EventUrl { get; set; }

        public EventCoacheeToCoachLeaveEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            UserEventAttendStatusChangeEmailDto eventAttendStatusDto,
            string eventUrl)
            : base(userNotificationSettingsUrl)
        {
            CoacheeFullName = eventAttendStatusDto.FullName;
            EventName = eventAttendStatusDto.EventName;
            EventUrl = eventUrl;
        }
    }
}
