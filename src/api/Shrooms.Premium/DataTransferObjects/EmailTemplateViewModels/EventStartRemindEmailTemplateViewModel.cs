using Shrooms.Contracts.DataTransferObjects;
using System;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class EventStartRemindEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public DateTime StartDate { get; set; }

        public EventStartRemindEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            string name,
            string url) : base(userNotificationSettingsUrl)
        {
            Name = name;
            Url = url;
        }
    }
}
