namespace Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels
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
            ServiceRequestTitle = serviceRequestTitle;
            FullName = fullName;
            Url = url;
        }
    }
}
