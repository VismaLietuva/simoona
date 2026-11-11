using Markdig;
using Markdig.Extensions.EmphasisExtras;
using Shrooms.Domain.Services.Wall.Mentions;

namespace Shrooms.Domain.Helpers
{
    public class MarkdigMarkdownConverter : IMarkdownConverter
    {
        private static readonly MarkdownPipeline Pipeline = BasePipeline().Build();

        private static readonly MarkdownPipeline EmailPipeline = BasePipeline().UseAutoLinks().Build();

        public string ConvertToHtml(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }

            return Markdown.ToHtml(ExpandMentions(markdown), Pipeline);
        }

        public string ConvertToHtml(string markdown, IReadOnlyDictionary<string, string> customEmojiImages, int? maxTextLength = null)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return string.Empty;
            }

            var document = Markdown.Parse(ExpandMentions(markdown), EmailPipeline);
            EmojiInlineRewriter.Apply(document, customEmojiImages);
            if (maxTextLength.HasValue)
            {
                MarkdownDocumentTruncator.Apply(document, maxTextLength.Value);
            }

            return Markdown.ToHtml(document, EmailPipeline);
        }

        private static MarkdownPipelineBuilder BasePipeline()
        {
            return new MarkdownPipelineBuilder()
                .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
                .UseSoftlineBreakAsHardlineBreak();
        }

        private static string ExpandMentions(string markdown)
        {
            return MentionTokenParser.Replace(markdown, token => $"**@{token.Label}**");
        }
    }
}
