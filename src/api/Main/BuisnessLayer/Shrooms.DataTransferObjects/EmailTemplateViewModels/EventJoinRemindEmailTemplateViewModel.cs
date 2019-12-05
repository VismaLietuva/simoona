using Shrooms.EntityModels.Models.Events;

namespace Shrooms.DataTransferObjects.EmailTemplateViewModels
{
    public class EventJoinRemindEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public EventJoinRemindEmailTemplateViewModel(string userNotificationSettingsUrl)
            : base(userNotificationSettingsUrl)
        {
        }

        public string EventTypeName { get; set; }

        public string EventPageUrl { get; set; }
    }
}
