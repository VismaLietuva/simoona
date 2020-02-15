namespace Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels
{
    public class NewCommentEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public NewCommentEmailTemplateViewModel(string postCommentTitle, string pictureUrl, string fullName, string postDeepLink, string messageBody, string userNotificationSettingsUrl, string actionButtonTitle)
            : base(userNotificationSettingsUrl)
        {
            PostCommentTitle = postCommentTitle;
            PictureUrl = pictureUrl;
            FullName = fullName;
            PostDeepLink = postDeepLink;
            MessageBody = messageBody;
            ActionButtonTitle = actionButtonTitle;
        }

        public string PostCommentTitle { get; set; }
        public string PictureUrl { get; set; }
        public string FullName { get; set; }
        public string PostDeepLink { get; set; }
        public string MessageBody { get; set; }
        public string ActionButtonTitle { get; set; }
    }
}
