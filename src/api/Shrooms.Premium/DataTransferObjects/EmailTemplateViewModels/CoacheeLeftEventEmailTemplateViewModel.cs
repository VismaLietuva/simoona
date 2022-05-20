using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Users;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class CoacheeLeftEventEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string CoacheeFullName { get; set; }

        public string EventName { get; set; }

        public string EventUrl { get; set; }

        public CoacheeLeftEventEmailTemplateViewModel(
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
