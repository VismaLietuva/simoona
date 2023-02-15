using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Infrastructure.Email.Attributes;
using System;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class SharedEventEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string PostUrl { get; set; }

        public string EventUrl { get; set; }

        public string SharerFullName { get; set; }

        public string PostBody { get; set; }

        public string WallName { get; set; }

        public string EventName { get; set; }

        [ApplyTimeZoneChanges]
        public DateTime StartDate { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public SharedEventEmailTemplateViewModel(
            string postUrl,
            string eventUrl,
            string sharerFullName,
            string postBody,
            string wallName,
            string eventName,
            DateTime startDate,
            string description,
            string location,
            string userNotificationSettingsUrl)
            :
            base(userNotificationSettingsUrl)
        {
            PostUrl = postUrl;
            SharerFullName = sharerFullName;
            PostBody = postBody;
            WallName = wallName;
            EventName = eventName;
            StartDate = startDate;
            Description = description;
            Location = location;
            EventUrl = eventUrl;
        }
    }
}
