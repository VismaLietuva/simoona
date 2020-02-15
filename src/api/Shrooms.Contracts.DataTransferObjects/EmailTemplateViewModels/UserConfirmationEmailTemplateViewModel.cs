namespace Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels
{
    public class UserConfirmationEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string MainPageUrl { get; private set; }
        public string Content { get; private set; }

        public UserConfirmationEmailTemplateViewModel(string userNotificationSettingsUrl, string mainPageUrl, string content)
            : base(userNotificationSettingsUrl)
        {
            MainPageUrl = mainPageUrl;
            Content = content;
        }
    }
}
