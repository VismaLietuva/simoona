using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class CommitteeSuggestionEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string CommitteeName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CommitteeUrl { get; set; }

        public CommitteeSuggestionEmailTemplateViewModel(string userNotificationSettingsUrl, string committeeName, string title, string description, string committeeUrl)
            : base(userNotificationSettingsUrl)
        {
            CommitteeName = committeeName;
            Title = title;
            Description = description;
            CommitteeUrl = committeeUrl;
        }
    }
}
