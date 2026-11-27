using System.Text;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Shrooms.Domain.Helpers
{
    internal static class ImageLinkRewriter
    {
        private static readonly HashSet<string> LinkableSchemes = new(StringComparer.OrdinalIgnoreCase) { "http", "https", "mailto" };

        public static void Apply(MarkdownDocument document)
        {
            var emptiedLinks = new HashSet<LinkInline>();

            foreach (var image in document.Descendants<LinkInline>().Where(link => link.IsImage).ToList())
            {
                if (image.Parent == null)
                {
                    continue;
                }

                var outerLink = image.Parent.FirstParentOfType<LinkInline>();
                if (outerLink == null)
                {
                    ReplaceWithLink(image);
                }
                else
                {
                    ReplaceWithText(image, AltText(image));
                    emptiedLinks.Add(outerLink);
                }
            }

            foreach (var link in emptiedLinks.Where(link => IsBlank(PlainText(link)) && IsLinkable(link.Url)))
            {
                link.AppendChild(new LiteralInline(link.Url));
            }
        }

        private static void ReplaceWithLink(LinkInline image)
        {
            var label = AltText(image);
            if (!IsLinkable(image.Url))
            {
                ReplaceWithText(image, label);
                return;
            }

            var link = new LinkInline(image.Url, image.Title);
            link.AppendChild(new LiteralInline(IsBlank(label) ? image.Url : label));
            image.ReplaceBy(link, copyChildren: false);
        }

        private static void ReplaceWithText(LinkInline image, string text)
        {
            if (IsBlank(text))
            {
                image.Remove();
                return;
            }

            image.ReplaceBy(new LiteralInline(text), copyChildren: false);
        }

        private static string AltText(LinkInline image)
        {
            return PlainText(image).Trim();
        }

        private static string PlainText(ContainerInline container)
        {
            var text = new StringBuilder();
            foreach (var inline in container.Descendants<Inline>())
            {
                text.Append(inline switch
                {
                    LiteralInline literal => literal.Content.ToString(),
                    CodeInline code => code.Content,
                    LineBreakInline => " ",
                    _ => string.Empty
                });
            }

            return text.ToString();
        }

        private static bool IsBlank(string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        private static bool IsLinkable(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) && LinkableSchemes.Contains(uri.Scheme);
        }
    }
}
