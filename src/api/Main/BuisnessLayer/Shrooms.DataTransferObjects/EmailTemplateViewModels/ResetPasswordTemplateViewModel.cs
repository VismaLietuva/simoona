using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.EmailTemplateViewModels
{
    public class ResetPasswordTemplateViewModel : BaseEmailTemplateViewModel
    {
        public ResetPasswordTemplateViewModel(string fullName, string userNotificationSettingsUrl, string resetUrl)
            : base(userNotificationSettingsUrl)
        {
            FullName = fullName;
            ResetUrl = resetUrl;
        }

        public string FullName { get; set; }

        public string ResetUrl { get; set; }
    }
}
