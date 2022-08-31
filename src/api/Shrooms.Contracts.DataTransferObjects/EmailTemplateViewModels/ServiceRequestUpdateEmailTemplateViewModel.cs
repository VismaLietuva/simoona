namespace Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels
{
    public class ServiceRequestUpdateEmailTemplateViewModel : ServiceRequestEmailTemplateViewModel
    {
        public string StatusName { get; set; }

        public ServiceRequestUpdateEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            string serviceRequestTitle,
            string fullName,
            string statusName,
            string url)
            : base(userNotificationSettingsUrl, serviceRequestTitle, fullName, url)
        {
            StatusName = statusName;
        }
    }
}
