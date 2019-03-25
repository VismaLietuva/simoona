using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.EmailTemplateViewModels
{
    public class VerifyEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public VerifyEmailTemplateViewModel(string fullName, string userNotificationSettingsUrl, string verifyUrl)
            : base(userNotificationSettingsUrl)
        {
            FullName = fullName;
            VerifyUrl = verifyUrl;
        }

        public string FullName { get; set; }

        public string VerifyUrl { get; set; }
    }
}
