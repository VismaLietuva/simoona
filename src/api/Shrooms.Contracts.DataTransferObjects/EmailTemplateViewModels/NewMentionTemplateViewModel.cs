namespace Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels
{
    public class NewMentionTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string Subject { get; set; }

        public string MentionedUsersFullName { get; set; }

        public string MentioningUsersFullName { get; set; }

        public string PostUrl { get; set; }

        public string Content { get; set; }

        public NewMentionTemplateViewModel(
            string subject,
            string mentionedUser,
            string mentioningUser,
            string postUrl,
            string settingsUrl,
            string content)
            : base(settingsUrl)
        {
            Subject = subject;
            MentionedUsersFullName = mentionedUser;
            MentioningUsersFullName = mentioningUser;
            PostUrl = postUrl;
            Content = content;
        }
    }
}
