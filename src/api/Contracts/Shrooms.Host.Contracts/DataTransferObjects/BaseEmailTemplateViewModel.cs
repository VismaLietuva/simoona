namespace Shrooms.Host.Contracts.DataTransferObjects
{
    public abstract class BaseEmailTemplateViewModel
    {
        public string UserNotificationSettingsUrl { get; private set; }

        protected BaseEmailTemplateViewModel(string userNotificationSettingsUrl)
        {
            UserNotificationSettingsUrl = userNotificationSettingsUrl;
        }
    }
}