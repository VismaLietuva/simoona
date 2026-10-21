using Shrooms.Contracts.DataTransferObjects.Models.Emoji;

namespace Shrooms.Domain.Helpers
{
    public interface IMarkdownConverter
    {
        string ConvertToHtml(string markdown);

        string ConvertToHtml(string markdown, IEnumerable<CustomEmojiDto> customEmojis, string imageBaseUrl);
    }
}
