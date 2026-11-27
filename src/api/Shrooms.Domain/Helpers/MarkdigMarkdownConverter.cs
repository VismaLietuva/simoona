using Markdig;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Shrooms.Domain.Services.Wall.Mentions;

namespace Shrooms.Domain.Helpers
{
    public class MarkdigMarkdownConverter : IMarkdownConverter
    {
        private static readonly MarkdownPipeline Pipeline = BasePipeline().Build();

        private static readonly MarkdownPipeline EmailPipeline = BasePipeline().UseAutoLinks().DisableHtml().Build();

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
            RenderImagesAsLinks(document);
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

        private static void RenderImagesAsLinks(MarkdownDocument document)
        {
            foreach (var image in document.Descendants<LinkInline>().Where(link => link.IsImage).ToList())
            {
                if (image.FirstChild == null)
                {
                    image.AppendChild(new LiteralInline(image.Url));
                }

                if (image.Parent.FirstParentOfType<LinkInline>() == null)
                {
                    image.IsImage = false;
                }
                else
                {
                    image.ReplaceBy(new ContainerInline());
                }
            }
        }

        private static string ExpandMentions(string markdown)
        {
            return MentionTokenParser.Replace(markdown, token => $"**@{token.Label}**");
        }
    }
}
