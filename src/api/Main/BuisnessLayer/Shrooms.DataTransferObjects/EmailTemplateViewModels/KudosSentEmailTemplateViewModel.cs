namespace Shrooms.DataTransferObjects.EmailTemplateViewModels
{
    public class KudosSentEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string KudosSenderName { get; set; }

        public decimal KudosAmount { get; set; }

        public string Comment { get; set; }

        public string KudosProfileUrl { get; set; }

        public KudosSentEmailTemplateViewModel(string userNotificationSettingsUrl, string kudosSenderName, decimal kudosAmount, string comment, string kudosProfileUrl)
            : base(userNotificationSettingsUrl)
        {
            KudosSenderName = kudosSenderName;
            KudosAmount = kudosAmount;
            Comment = comment;
            KudosProfileUrl = kudosProfileUrl;
        }
    }
}
