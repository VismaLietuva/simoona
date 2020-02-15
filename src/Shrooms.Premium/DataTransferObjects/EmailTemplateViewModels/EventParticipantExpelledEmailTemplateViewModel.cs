using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class EventParticipantExpelledEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string EventTitle { get; set; }
        public string EventUrl { get; set; }

        public EventParticipantExpelledEmailTemplateViewModel(string userNotificationSettingsUrl, string eventTitle, string eventUrl)
            : base(userNotificationSettingsUrl)
        {
            EventTitle = eventTitle;
            EventUrl = eventUrl;
        }
    }
}
