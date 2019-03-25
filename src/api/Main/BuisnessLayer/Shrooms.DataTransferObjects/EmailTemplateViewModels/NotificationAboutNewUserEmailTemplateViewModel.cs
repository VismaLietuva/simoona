namespace Shrooms.DataTransferObjects.EmailTemplateViewModels
{
    public class NotificationAboutNewUserEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string NewUserProfileUrl { get; set; }
        public string NewUserName { get; set; }

        public NotificationAboutNewUserEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            string newUserProfileUrl, 
            string newUserName)
            : base(userNotificationSettingsUrl)
        {
            this.NewUserProfileUrl = newUserProfileUrl;
            this.NewUserName = newUserName;
        }
    }
}
