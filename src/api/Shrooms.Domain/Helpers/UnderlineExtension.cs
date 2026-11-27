using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;

namespace Shrooms.Domain.Helpers
{
    internal sealed class UnderlineExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer.ObjectRenderers.FindExact<EmphasisInlineRenderer>() is not { } emphasisRenderer)
            {
                return;
            }

            var defaultTag = emphasisRenderer.GetTag;
            emphasisRenderer.GetTag = emphasis =>
                emphasis.DelimiterChar == '_' && emphasis.DelimiterCount == 2 ? "u" : defaultTag(emphasis);
        }
    }
}
