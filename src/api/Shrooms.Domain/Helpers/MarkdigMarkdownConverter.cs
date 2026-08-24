using Markdig;
using Markdig.Extensions.EmphasisExtras;
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

            var withMentions = MentionTokenParser.Replace(markdown, token => $"**@{token.Label}**");

            return Markdown.ToHtml(withMentions, Pipeline);
        }
    }
}
