namespace Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels
{
    public class NewWallPostEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public NewWallPostEmailTemplateViewModel(string wallTitle, string wallName, string eyebrow, string pictureUrl, string fullName, string postDeepLink, string messageBody, string userNotificationSettingsUrl, string actionButtonTitle)
            : base(userNotificationSettingsUrl)
        {
            WallTitle = wallTitle;
            WallName = wallName;
            Eyebrow = eyebrow;
            PictureUrl = pictureUrl;
            FullName = fullName;
            PostDeepLink = postDeepLink;
            MessageBody = messageBody;
            ActionButtonTitle = actionButtonTitle;
        }

        public string WallTitle { get; set; }
        public string WallName { get; set; }
        public string Eyebrow { get; set; }
        public string PictureUrl { get; set; }
        public string FullName { get; set; }
        public string PostDeepLink { get; set; }
        public string MessageBody { get; set; }
        public string ActionButtonTitle { get; set; }
    }
}
