using Microsoft.Extensions.Logging;
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
        private readonly ILogger<EmailMarkdownRenderer> _logger;

        public EmailMarkdownRenderer(
            IMarkdownConverter markdownConverter,
            IMentionLinkExpander mentionLinkExpander,
            ICustomEmojiService customEmojiService,
            IApplicationSettings appSettings,
            ILogger<EmailMarkdownRenderer> logger)
        {
            _markdownConverter = markdownConverter;
            _mentionLinkExpander = mentionLinkExpander;
            _customEmojiService = customEmojiService;
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task<string> RenderAsync(string markdown, int organizationId, string organizationShortName, int? maxTextLength = null)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }

            var emojis = EmojiInlineRewriter.ContainsToken(markdown) ? await LoadEmojisAsync(organizationId, organizationShortName) : null;
            var withMentionLinks = _mentionLinkExpander.ExpandToMarkdown(markdown, organizationShortName);

            return _markdownConverter.ConvertToHtml(withMentionLinks, ToImageUrls(emojis), maxTextLength);
        }

        private async Task<IEnumerable<CustomEmojiDto>> LoadEmojisAsync(int organizationId, string organizationShortName)
        {
            try
            {
                return (await _customEmojiService.GetAllAsync(organizationId, organizationShortName)).Emojis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Custom emoji lookup failed for organization {OrganizationId}; rendering the email without custom emoji", organizationId);
                return null;
            }
        }

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
