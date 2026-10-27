using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Shrooms.Domain.Helpers
{
    // Replaces :shortcode: tokens in a parsed document with the same rules as the web app's
    // scanner, so a post renders alike on the wall and in the email:
    //
    // - a standard name wins over a custom one, as in Slack; an unknown name stays literal;
    // - a token glued to a word does not count. Letters, digits, marks and colons glue, so a
    //   colon chain such as status:ok:200 or fe80::a:1 is one word. Glue is judged on the text
    //   the reader sees, across emphasis, code spans and links: **word**:tada: stays literal;
    // - a token right after an accepted or unknown token is fine (:nope::tada: renders the second);
    // - code spans, autolink labels and image alt text are never rewritten.
    internal static partial class EmojiInlineRewriter
    {
        private const int ToneVariantCount = 6;

        [GeneratedRegex(@":([a-z0-9_+-]{1,60}):")]
        private static partial Regex Token();

        [GeneratedRegex(@"\G:skin-tone-([2-6]):")]
        private static partial Regex Tone();

        // One inline leaf of a block in reading order: a literal to scan, text that only gives
        // glue context, or a boundary (break, image, raw HTML) that resets the context.
        private readonly record struct Leaf(LiteralInline Scan, string Text)
        {
            public static Leaf Boundary => default;

            public bool IsBoundary => Scan == null && Text == null;
        }

        public static void Apply(MarkdownDocument document, IReadOnlyDictionary<string, string> customEmojiImages)
        {
            var custom = customEmojiImages ?? ReadOnlyDictionary<string, string>.Empty;
            var leaves = new List<Leaf>();
            var run = new List<LiteralInline>();

            foreach (var block in document.Descendants<LeafBlock>())
            {
                if (block.Inline == null)
                {
                    continue;
                }

                leaves.Clear();
                Collect(block.Inline, true, leaves);
                Rewrite(leaves, run, custom);
            }
        }

        private static void Collect(ContainerInline container, bool scannable, List<Leaf> leaves)
        {
            for (var child = container.FirstChild; child != null; child = child.NextSibling)
            {
                switch (child)
                {
                    case LiteralInline literal:
                        leaves.Add(scannable ? new Leaf(literal, null) : new Leaf(null, literal.Content.ToString()));
                        break;
                    case CodeInline code:
                        leaves.Add(new Leaf(null, code.Content));
                        break;
                    case HtmlEntityInline entity:
                        leaves.Add(new Leaf(null, entity.Transcoded.ToString()));
                        break;
                    case AutolinkInline autolink:
                        leaves.Add(new Leaf(null, autolink.Url));
                        break;
                    case LinkInline { IsImage: true }:
                        leaves.Add(Leaf.Boundary);
                        break;
                    case LinkInline link:
                        Collect(link, scannable && !link.IsAutoLink, leaves);
                        break;
                    case ContainerInline nested:
                        Collect(nested, scannable, leaves);
                        break;
                    default:
                        leaves.Add(Leaf.Boundary);
                        break;
                }
            }
        }

        private static void Rewrite(List<Leaf> leaves, List<LiteralInline> run, IReadOnlyDictionary<string, string> custom)
        {
            var before = string.Empty;
            var index = 0;

            while (index < leaves.Count)
            {
                var leaf = leaves[index];
                if (leaf.Scan == null)
                {
                    before = leaf.IsBoundary ? string.Empty : leaf.Text.Length > 0 ? leaf.Text : before;
                    index++;
                    continue;
                }

                // Consecutive sibling literals form one run: Markdig splits text at unmatched
                // delimiters, so a token such as :heart_eyes: can span several of them.
                run.Clear();
                var hasColon = false;
                var parent = leaf.Scan.Parent;
                while (index < leaves.Count && leaves[index].Scan is { } literal && ReferenceEquals(literal.Parent, parent))
                {
                    run.Add(literal);
                    hasColon |= literal.Content.IndexOf(':') >= 0;
                    index++;
                }

                // Most text carries no shortcode; without a colon only the glue context moves on.
                if (!hasColon)
                {
                    var tail = run[^1].Content;
                    before = tail.Length > 0 ? tail.ToString() : before;
                    continue;
                }

                var text = run.Count == 1
                    ? run[0].Content.ToString()
                    : string.Concat(run.Select(literal => literal.Content.ToString()));

                var replacement = Scan(text, before, TextAfter(leaves, index), custom);
                if (replacement == null)
                {
                    before = text;
                    continue;
                }

                foreach (var inline in replacement)
                {
                    run[0].InsertBefore(inline);
                }

                foreach (var literal in run)
                {
                    literal.Remove();
                }

                before = replacement[^1] is LiteralInline last ? last.Content.ToString() : string.Empty;
            }
        }

        private static string TextAfter(List<Leaf> leaves, int index)
        {
            for (var i = index; i < leaves.Count; i++)
            {
                var leaf = leaves[i];
                if (leaf.IsBoundary)
                {
                    return string.Empty;
                }

                var text = leaf.Scan != null ? leaf.Scan.Content.ToString() : leaf.Text;
                if (text.Length > 0)
                {
                    return text;
                }
            }

            return string.Empty;
        }

        private static List<Inline> Scan(string text, string before, string after, IReadOnlyDictionary<string, string> custom)
        {
            List<Inline> output = null;
            var last = 0;
            var boundary = -1;
            var start = 0;

            while (start < text.Length)
            {
                var match = Token().Match(text, start);
                if (!match.Success)
                {
                    break;
                }

                var name = match.Groups[1].Value;
                var end = match.Index + match.Length;

                if (match.Index != boundary && IsGlueBefore(text, match.Index, before))
                {
                    start = match.Index + 1;
                    continue;
                }

                var emoji = Resolve(text, name, custom, ref end);

                if (IsWordAfter(text, end, after))
                {
                    start = match.Index + 1;
                    continue;
                }

                if (emoji != null)
                {
                    output ??= new List<Inline>();
                    if (match.Index > last)
                    {
                        output.Add(new LiteralInline(text[last..match.Index]));
                    }

                    output.Add(emoji);
                    last = end;
                }

                boundary = end;
                start = end;
            }

            if (output != null && last < text.Length)
            {
                output.Add(new LiteralInline(text[last..]));
            }

            return output;
        }

        // Returns the inline that replaces the token, or null when the name is unknown. A standard
        // tone-capable emoji also consumes a following :skin-tone-N: and moves end past it.
        private static Inline Resolve(string text, string name, IReadOnlyDictionary<string, string> custom, ref int end)
        {
            if (StandardEmojiTable.TryGet(name, out var variants))
            {
                var tone = 0;
                if (variants.Count == ToneVariantCount)
                {
                    var toneMatch = Tone().Match(text, end);
                    if (toneMatch.Success)
                    {
                        tone = toneMatch.Groups[1].Value[0] - '0';
                        end += toneMatch.Length;
                    }
                }

                return new LiteralInline(variants[tone > 0 ? tone - 1 : 0]);
            }

            return custom.TryGetValue(name, out var imageUrl) ? new HtmlInline(ImageTag(name, imageUrl)) : null;
        }

        private static string ImageTag(string name, string imageUrl)
        {
            var src = WebUtility.HtmlEncode(imageUrl);

            return $"<img src=\"{src}\" alt=\":{name}:\" title=\":{name}:\" width=\"20\" height=\"20\" style=\"vertical-align:-4px\">";
        }

        private static bool IsGlueBefore(string text, int index, string before)
        {
            return index == 0 ? EndsGlued(before) : EndsGlued(text, index);
        }

        private static bool IsWordAfter(string text, int end, string after)
        {
            return end < text.Length ? IsWordAt(text, end) : after.Length > 0 && IsWordAt(after, 0);
        }

        private static bool EndsGlued(string text)
        {
            return EndsGlued(text, text.Length);
        }

        // Whether the code point just before text[end] is a word character or a colon.
        private static bool EndsGlued(string text, int end)
        {
            if (end == 0)
            {
                return false;
            }

            var previous = end >= 2 && char.IsLowSurrogate(text[end - 1]) ? end - 2 : end - 1;
            return text[previous] == ':' || IsWordAt(text, previous);
        }

        private static bool IsWordAt(string text, int index)
        {
            return CharUnicodeInfo.GetUnicodeCategory(text, index) switch
            {
                UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or UnicodeCategory.TitlecaseLetter
                    or UnicodeCategory.ModifierLetter or UnicodeCategory.OtherLetter
                    or UnicodeCategory.DecimalDigitNumber or UnicodeCategory.LetterNumber or UnicodeCategory.OtherNumber
                    or UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.EnclosingMark => true,
                _ => false
            };
        }
    }
}
