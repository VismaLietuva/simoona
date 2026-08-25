namespace Shrooms.Contracts.DataTransferObjects
{
    public abstract class BaseEmailTemplateViewModel
    {
        public string UserNotificationSettingsUrl { get; private set; }

        // Set by MailTemplate from configuration, not by the callers building the model.
        public string HomeUrl { get; set; }

        protected BaseEmailTemplateViewModel(string userNotificationSettingsUrl)
        {
            UserNotificationSettingsUrl = userNotificationSettingsUrl;
        }
    }
}