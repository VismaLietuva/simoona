using Shrooms.Contracts.DataTransferObjects.Models.Emoji;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Emoji;
using Shrooms.Domain.Services.Wall.Mentions;

namespace Shrooms.Domain.Services.Email
{
    public class EmailMarkdownRenderer : IEmailMarkdownRenderer
    {
        private readonly IMarkdownConverter _markdownConverter;
        private readonly IMentionLinkExpander _mentionLinkExpander;
        private readonly ICustomEmojiService _customEmojiService;
        private readonly IApplicationSettings _appSettings;

        public EmailMarkdownRenderer(
            IMarkdownConverter markdownConverter,
            IMentionLinkExpander mentionLinkExpander,
            ICustomEmojiService customEmojiService,
            IApplicationSettings appSettings)
        {
            _markdownConverter = markdownConverter;
            _mentionLinkExpander = mentionLinkExpander;
            _customEmojiService = customEmojiService;
            _appSettings = appSettings;
        }

        public async Task<string> RenderAsync(string markdown, int organizationId, string organizationShortName)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }

            var emojis = markdown.Contains(':') ? await _customEmojiService.GetAllAsync(organizationId, organizationShortName) : null;
            var withMentionLinks = _mentionLinkExpander.ExpandToMarkdown(markdown, organizationShortName);

            return _markdownConverter.ConvertToHtml(withMentionLinks, ToImageUrls(emojis?.Emojis));
        }

        // CustomEmojiDto.Url is relative to the API host, which is what the web client expects.
        // A mail client has no such base, so the image needs the absolute address.
        private IReadOnlyDictionary<string, string> ToImageUrls(IEnumerable<CustomEmojiDto> emojis)
        {
            var images = new Dictionary<string, string>(StringComparer.Ordinal);
            if (emojis == null)
            {
                return images;
            }

            var apiUrl = (_appSettings.ApiUrl ?? string.Empty).TrimEnd('/');
            foreach (var emoji in emojis)
            {
                if (!string.IsNullOrEmpty(emoji?.Name))
                {
                    images.TryAdd(emoji.Name, apiUrl + emoji.Url);
                }
            }

            return images;
        }
    }
}
