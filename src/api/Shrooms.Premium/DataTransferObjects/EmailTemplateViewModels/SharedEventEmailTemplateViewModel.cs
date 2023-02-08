using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Infrastructure.Email.Attributes;
using System;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class SharedEventEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string PostUrl { get; set; }

        public string SharerFullName { get; set; }

        public string PostBody { get; set; }

        public string WallName { get; set; }

        public string EventName { get; set; }

        [ApplyTimeZoneChanges]
        public DateTime StartDate { get; set; }

        [ApplyTimeZoneChanges]
        public DateTime EndDate { get; set; }

        [ApplyTimeZoneChanges]
        public DateTime RegistrationDeadlineDate { get; set; }

        public string TypeName { get; set; }

        public string Description { get; set; }

        public SharedEventEmailTemplateViewModel(
            string postUrl,
            string sharerFullName,
            string postBody,
            string wallName,
            string eventName,
            DateTime startDate,
            DateTime endDate,
            DateTime registrationDeadlineDate,
            string typeName,
            string description,
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
            EndDate = endDate;
            RegistrationDeadlineDate = registrationDeadlineDate;
            TypeName = typeName;
            Description = description;
        }
    }
}
