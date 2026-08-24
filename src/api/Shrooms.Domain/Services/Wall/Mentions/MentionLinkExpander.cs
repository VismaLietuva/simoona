using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Domain.Services.Wall.Mentions
{
    public class MentionLinkExpander : IMentionLinkExpander
    {
        private readonly IApplicationSettings _appSettings;

        public MentionLinkExpander(IApplicationSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public string ExpandToMarkdown(string messageBody, string organizationShortName)
        {
            return MentionTokenParser.Replace(messageBody, token =>
            {
                var label = $"@{token.Label}";

                if (string.IsNullOrEmpty(organizationShortName))
                {
                    return $"**{label}**";
                }

                var url = token.Kind == MentionKind.User
                    ? _appSettings.UserProfileUrl(organizationShortName, token.UserId)
                    : _appSettings.GroupUrl(organizationShortName, token.GroupId);

                return $"[{label}]({url})";
            });
        }
    }
}
