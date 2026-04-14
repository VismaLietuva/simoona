using Markdig;
using Markdig.Extensions.EmphasisExtras;

namespace Shrooms.Domain.Helpers
{
    public class MarkdigMarkdownConverter : IMarkdownConverter
    {
        private static readonly MarkdownPipeline Pipeline =
            new MarkdownPipelineBuilder()
                .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
                .UseSoftlineBreakAsHardlineBreak()
                .Build();

        public string ConvertToHtml(string markdown) =>
            string.IsNullOrEmpty(markdown) ? string.Empty : Markdown.ToHtml(markdown, Pipeline);
    }
}
