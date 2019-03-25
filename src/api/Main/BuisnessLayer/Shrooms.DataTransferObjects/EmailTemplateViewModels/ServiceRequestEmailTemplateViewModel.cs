using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.EmailTemplateViewModels
{
    public class ServiceRequestEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string ServiceRequestTitle { get; set; }
        public string FullName { get; set; }
        public string Url { get; set; }

        public ServiceRequestEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            string serviceRequestTitle,
            string fullName,
            string url)
            : base(userNotificationSettingsUrl)
        {
            this.ServiceRequestTitle = serviceRequestTitle;
            this.FullName = fullName;
            this.Url = url;
        }
    }
}
