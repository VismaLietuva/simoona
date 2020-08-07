
namespace Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels
{
    public class NewMentionTemplateViewModel : BaseEmailTemplateViewModel
    {
        public NewMentionTemplateViewModel(string mentionedUser, string mentioningUser, string postUrl, string settingsUrl, string content) : base(settingsUrl)
        {
            MentionedUsersFullName = mentionedUser;
            MentioningUsersFullName = mentioningUser;
            PostUrl = postUrl;
            Content = content;
        }

        public string MentionedUsersFullName { get; set; }
        public string MentioningUsersFullName { get; set; }
        public string PostUrl { get; set; }
        public string Content { get; set; }
    }
}
