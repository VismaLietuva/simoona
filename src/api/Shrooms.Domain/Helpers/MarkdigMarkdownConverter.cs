using Markdig;
using Markdig.Extensions.EmphasisExtras;
using Shrooms.Domain.Services.Wall.Mentions;

namespace Shrooms.Domain.Helpers
{
    public class MarkdigMarkdownConverter : IMarkdownConverter
    {
        // Bare URLs become links, as on the wall. The emoji rewriter leaves autolink labels alone,
        // so a token inside a URL is never turned into an emoji.
        private static readonly MarkdownPipeline Pipeline =
            new MarkdownPipelineBuilder()
                .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
                .UseSoftlineBreakAsHardlineBreak()
                .UseAutoLinks()
                .Build();

        public string ConvertToHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }

            return Markdown.ToHtml(ExpandMentions(markdown), Pipeline);
        }

        public string ConvertToHtml(string markdown, IReadOnlyDictionary<string, string> customEmojiImages)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }

            var document = Markdown.Parse(ExpandMentions(markdown), Pipeline);
            EmojiInlineRewriter.Apply(document, customEmojiImages);

            return Markdown.ToHtml(document, Pipeline);
        }

        private static string ExpandMentions(string markdown)
        {
            return MentionTokenParser.Replace(markdown, token => $"**@{token.Label}**");
        }
    }
}
