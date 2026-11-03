using System.Globalization;
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Shrooms.Domain.Helpers
{
    internal static class MarkdownDocumentTruncator
    {
        private const string Ellipsis = "...";

        public static void Apply(MarkdownDocument document, int maxTextLength)
        {
            var remaining = maxTextLength;
            LeafBlock previousLeaf = null;

            foreach (var leaf in document.Descendants<LeafBlock>().ToList())
            {
                if (leaf.Inline == null)
                {
                    var length = RawLength(leaf);
                    if (length > remaining)
                    {
                        RemoveBlockAndFollowing(leaf);
                        AppendEllipsis(document, previousLeaf);
                        return;
                    }

                    remaining -= length;
                    previousLeaf = leaf;
                    continue;
                }

                var keptSomething = false;
                foreach (var inline in Leaves(leaf.Inline).ToList())
                {
                    var length = TextLength(inline);
                    if (length <= remaining)
                    {
                        remaining -= length;
                        keptSomething |= length > 0;
                        continue;
                    }

                    if (remaining > 0 && inline is LiteralInline literal)
                    {
                        literal.Content = new StringSlice(Prefix(literal.Content.ToString(), remaining));
                        RemoveFollowing(literal, leaf.Inline, false);
                    }
                    else if (keptSomething)
                    {
                        RemoveFollowing(inline, leaf.Inline, true);
                    }
                    else
                    {
                        RemoveBlockAndFollowing(leaf);
                        AppendEllipsis(document, previousLeaf);
                        return;
                    }

                    RemoveBlocksAfter(leaf);
                    AppendEllipsis(document, leaf);
                    return;
                }

                previousLeaf = leaf;
            }
        }

        private static IEnumerable<Inline> Leaves(ContainerInline container)
        {
            for (var child = container.FirstChild; child != null; child = child.NextSibling)
            {
                if (child is ContainerInline nested and not LinkInline { IsImage: true })
                {
                    foreach (var leaf in Leaves(nested))
                    {
                        yield return leaf;
                    }
                }
                else
                {
                    yield return child;
                }
            }
        }

        private static int TextLength(Inline inline)
        {
            return inline switch
            {
                LiteralInline literal => Graphemes(literal.Content.ToString()),
                CodeInline code => Graphemes(code.Content),
                HtmlEntityInline entity => Graphemes(entity.Transcoded.ToString()),
                AutolinkInline autolink => autolink.Url.Length,
                LineBreakInline or HtmlInline or LinkInline => 1,
                _ => 0
            };
        }

        private static int RawLength(LeafBlock leaf)
        {
            return leaf is CodeBlock code ? code.Lines.ToString().Length : 0;
        }

        private static int Graphemes(string text)
        {
            return new StringInfo(text).LengthInTextElements;
        }

        private static string Prefix(string text, int maxTextElements)
        {
            var end = 0;
            for (var i = 0; i < maxTextElements && end < text.Length; i++)
            {
                end += StringInfo.GetNextTextElementLength(text, end);
            }

            return text[..end];
        }

        private static void RemoveFollowing(Inline inline, ContainerInline root, bool includeSelf)
        {
            var current = inline;
            while (current != null && !ReferenceEquals(current, root))
            {
                while (current.NextSibling != null)
                {
                    current.NextSibling.Remove();
                }

                var parent = current.Parent;
                if (includeSelf)
                {
                    current.Remove();
                    includeSelf = parent != null && !ReferenceEquals(parent, root) && parent.FirstChild == null;
                }

                current = parent;
            }

            while (root.LastChild is LineBreakInline trailingBreak)
            {
                trailingBreak.Remove();
            }
        }

        private static void RemoveBlocksAfter(Block block)
        {
            RemoveBlocks(block, false);
        }

        private static void RemoveBlockAndFollowing(Block block)
        {
            RemoveBlocks(block, true);
        }

        private static void RemoveBlocks(Block block, bool includeSelf)
        {
            var current = block;
            while (current.Parent != null)
            {
                var parent = current.Parent;
                var index = parent.IndexOf(current);
                while (parent.Count > index + 1)
                {
                    parent.RemoveAt(parent.Count - 1);
                }

                if (includeSelf)
                {
                    parent.RemoveAt(index);
                    includeSelf = parent.Count == 0 && parent is not MarkdownDocument;
                }

                current = parent;
            }
        }

        private static void AppendEllipsis(MarkdownDocument document, LeafBlock leaf)
        {
            if (leaf?.Inline != null)
            {
                leaf.Inline.AppendChild(new LiteralInline(Ellipsis));
                return;
            }

            var paragraph = new ParagraphBlock { Inline = new ContainerInline() };
            paragraph.Inline.AppendChild(new LiteralInline(Ellipsis));
            document.Add(paragraph);
        }
    }
}
