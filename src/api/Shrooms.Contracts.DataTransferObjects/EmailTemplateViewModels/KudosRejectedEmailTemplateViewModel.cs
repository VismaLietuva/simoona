namespace Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels
{
    public class KudosRejectedEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string KudosRecipientName { get; set; }
        public decimal KudosAmount { get; set; }
        public string KudosType { get; set; }
        public string Comment { get; set; }
        public string RejectionReason { get; set; }
        public string KudosProfileUrl { get; set; }

        public KudosRejectedEmailTemplateViewModel(
            string userNotificationSettingsUrl,
            string kudosRecipientName,
            decimal kudosAmount,
            string kudosType,
            string comment,
            string rejectionReason,
            string kudosProfileUrl)
            : base(userNotificationSettingsUrl)
        {
            KudosRecipientName = kudosRecipientName;
            KudosAmount = kudosAmount;
            KudosType = kudosType;
            Comment = comment;
            RejectionReason = rejectionReason;
            KudosProfileUrl = kudosProfileUrl;
        }
    }
}
