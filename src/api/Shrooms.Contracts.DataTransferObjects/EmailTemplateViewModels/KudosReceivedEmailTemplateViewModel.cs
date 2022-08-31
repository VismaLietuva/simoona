namespace Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels
{
    public class KudosReceivedEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public decimal KudosAmount { get; set; }
        public string KudosType { get; set; }
        public string KudosSenderName { get; set; }
        public string Comment { get; set; }
        public string KudosProfileUrl { get; set; }

        public KudosReceivedEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            decimal kudosAmount,
            string kudosType,
            string kudosSenderName,
            string comment,
            string kudosProfileUrl)
            : base(userNotificationSettingsUrl)
        {
            KudosAmount = kudosAmount;
            KudosType = kudosType;
            KudosSenderName = kudosSenderName;
            Comment = comment;
            KudosProfileUrl = kudosProfileUrl;
        }
    }
}
