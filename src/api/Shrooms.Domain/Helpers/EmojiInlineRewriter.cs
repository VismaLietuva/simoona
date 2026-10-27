using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Shrooms.Domain.Helpers
{
    // Replaces :shortcode: tokens in a parsed document. A custom emoji (name -> image URL) wins
    // over a standard name, an unknown name stays literal, and text inside code spans and link
    // labels is left alone.
    internal static partial class EmojiInlineRewriter
    {
        private const int ToneVariantCount = 6;

        [GeneratedRegex(@":([a-z0-9_+-]{1,60}):")]
        private static partial Regex Token();

        [GeneratedRegex(@"\G:skin-tone-([2-6]):")]
        private static partial Regex Tone();

        public static void Apply(MarkdownDocument document, IReadOnlyDictionary<string, string> customEmojiImages)
        {
            var custom = customEmojiImages ?? ReadOnlyDictionary<string, string>.Empty;
            var run = new List<LiteralInline>();

            foreach (var block in document.Descendants<LeafBlock>())
            {
                if (block.Inline != null)
                {
                    RewriteContainer(block.Inline, custom, run);
                }
            }
        }

        // Adjacent literals form one run so a token Markdig split across them still matches. The run
        // list is shared with nested containers: it is always empty when the recursion starts.
        private static void RewriteContainer(ContainerInline container, IReadOnlyDictionary<string, string> custom, List<LiteralInline> run)
        {
            var child = container.FirstChild;
            while (child != null)
            {
                var next = child.NextSibling;

                if (child is LiteralInline literal)
                {
                    run.Add(literal);
                }
                else
                {
                    RewriteRun(run, custom);

                    if (child is ContainerInline nested and not LinkInline)
                    {
                        RewriteContainer(nested, custom, run);
                    }
                }

                child = next;
            }

            RewriteRun(run, custom);
        }

        private static void RewriteRun(List<LiteralInline> run, IReadOnlyDictionary<string, string> custom)
        {
            if (run.Count == 0)
            {
                return;
            }

            // Most text carries no shortcode. Without a colon there is nothing to match, so skip
            // building the string and running the regex.
            if (run.Exists(literal => literal.Content.IndexOf(':') >= 0))
            {
                var text = run.Count == 1
                    ? run[0].Content.ToString()
                    : string.Concat(run.Select(literal => literal.Content.ToString()));

                var replacement = Scan(text, custom);
                if (replacement != null)
                {
                    foreach (var inline in replacement)
                    {
                        run[0].InsertBefore(inline);
                    }

                    foreach (var literal in run)
                    {
                        literal.Remove();
                    }
                }
            }

            run.Clear();
        }

        private static List<Inline> Scan(string text, IReadOnlyDictionary<string, string> custom)
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

                if (match.Index != boundary && match.Index > 0 && IsGlueBefore(text, match.Index))
                {
                    start = match.Index + 1;
                    continue;
                }

                var emoji = Resolve(text, name, custom, ref end);

                if (end < text.Length && IsWordAt(text, end))
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
            if (custom.TryGetValue(name, out var imageUrl))
            {
                return new HtmlInline(ImageTag(name, imageUrl));
            }

            if (!StandardEmojiTable.TryGet(name, out var variants))
            {
                return null;
            }

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

        private static string ImageTag(string name, string imageUrl)
        {
            var src = WebUtility.HtmlEncode(imageUrl);

            return $"<img src=\"{src}\" alt=\":{name}:\" title=\":{name}:\" width=\"20\" height=\"20\" style=\"vertical-align:-4px\">";
        }

        private static bool IsGlueBefore(string text, int index)
        {
            var previous = char.IsLowSurrogate(text[index - 1]) && index >= 2 ? index - 2 : index - 1;
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
