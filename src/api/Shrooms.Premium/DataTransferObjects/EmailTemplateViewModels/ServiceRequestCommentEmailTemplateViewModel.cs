namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class ServiceRequestCommentEmailTemplateViewModel : ServiceRequestEmailTemplateViewModel
    {
        public string Comment { get; set; }
        public ServiceRequestCommentEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            string serviceRequestTitle,
            string fullName,
            string comment,
            string url)
            : base(userNotificationSettingsUrl, serviceRequestTitle, fullName, url)
        {
            Comment = comment;
        }
    }
}
