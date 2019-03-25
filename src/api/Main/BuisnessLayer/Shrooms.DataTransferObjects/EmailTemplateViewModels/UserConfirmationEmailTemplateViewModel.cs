namespace Shrooms.DataTransferObjects.EmailTemplateViewModels
{
    public class UserConfirmationEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string MainPageUrl { get; private set; }
        public string Content { get; private set; }

        public UserConfirmationEmailTemplateViewModel(string userNotificationSettingsUrl, string mainPageUrl, string content)
            : base(userNotificationSettingsUrl)
        {
            this.MainPageUrl = mainPageUrl;
            this.Content = content;
        }
    }
}
