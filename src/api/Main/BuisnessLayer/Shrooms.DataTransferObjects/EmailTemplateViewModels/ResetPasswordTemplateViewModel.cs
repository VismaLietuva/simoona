using Shrooms.Host.Contracts.DataTransferObjects;

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
