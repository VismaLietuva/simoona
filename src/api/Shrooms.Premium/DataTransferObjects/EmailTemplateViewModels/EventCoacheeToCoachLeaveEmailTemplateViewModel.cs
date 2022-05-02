using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class EventCoacheeToCoachLeaveEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string CoacheeFullName { get; set; }

        public string EventName { get; set; }

        public string EventUrl { get; set; }

        public EventCoacheeToCoachLeaveEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            string coacheeFullName,
            string eventName,
            string eventUrl)
            : base(userNotificationSettingsUrl)
        {
            CoacheeFullName = coacheeFullName;
            EventName = eventName;
            EventUrl = eventUrl;
        }
    }
}
