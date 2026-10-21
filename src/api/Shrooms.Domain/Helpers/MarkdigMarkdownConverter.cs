using Markdig;
using Markdig.Extensions.EmphasisExtras;
using Shrooms.Contracts.DataTransferObjects.Models.Emoji;
using Shrooms.Domain.Services.Wall.Mentions;

namespace Shrooms.Domain.Helpers
{
    public class MarkdigMarkdownConverter : IMarkdownConverter
    {
        private static readonly MarkdownPipeline Pipeline =
            new MarkdownPipelineBuilder()
                .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
                .UseSoftlineBreakAsHardlineBreak()
                .Build();

        public string ConvertToHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }

            return Markdown.ToHtml(ExpandMentions(markdown), Pipeline);
        }

        public string ConvertToHtml(string markdown, IEnumerable<CustomEmojiDto> customEmojis, string imageBaseUrl)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }

            var document = Markdown.Parse(ExpandMentions(markdown), Pipeline);
            EmojiInlineRewriter.Apply(document, customEmojis, imageBaseUrl);

            return Markdown.ToHtml(document, Pipeline);
        }

        private static string ExpandMentions(string markdown)
        {
            return MentionTokenParser.Replace(markdown, token => $"**@{token.Label}**");
        }
    }
}
