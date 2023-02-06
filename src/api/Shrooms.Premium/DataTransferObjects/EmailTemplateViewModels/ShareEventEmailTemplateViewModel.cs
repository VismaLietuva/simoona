using Shrooms.Contracts.DataTransferObjects;
using System;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class ShareEventEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public DateTime StartDate { get; set; }

        public ShareEventEmailTemplateViewModel(DateTime startDate, string userNotificationSettingsUrl)
            :
            base(userNotificationSettingsUrl)
        {
            StartDate = startDate;
        }
    }
}
