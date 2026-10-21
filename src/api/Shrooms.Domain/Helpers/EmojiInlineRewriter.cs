using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Shrooms.Contracts.DataTransferObjects.Models.Emoji;

namespace Shrooms.Domain.Helpers
{
    internal static class EmojiInlineRewriter
    {
        private const int ToneVariantCount = 6;

        private static readonly Regex Token = new(@":([a-z0-9_+-]{1,60}):", RegexOptions.Compiled);

        private static readonly Regex Tone = new(@"\G:skin-tone-([2-6]):", RegexOptions.Compiled);

        public static void Apply(MarkdownDocument document, IEnumerable<CustomEmojiDto> customEmojis, string imageBaseUrl)
        {
            var custom = (customEmojis ?? Array.Empty<CustomEmojiDto>())
                .Where(emoji => !string.IsNullOrEmpty(emoji?.Name))
                .GroupBy(emoji => emoji.Name, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            var baseUrl = imageBaseUrl?.TrimEnd('/') ?? string.Empty;

            foreach (var block in document.Descendants<LeafBlock>().ToList())
            {
                if (block.Inline != null)
                {
                    RewriteContainer(block.Inline, custom, baseUrl);
                }
            }
        }

        private static void RewriteContainer(ContainerInline container, Dictionary<string, CustomEmojiDto> custom, string baseUrl)
        {
            var run = new List<LiteralInline>();

            foreach (var child in container.ToList())
            {
                if (child is LiteralInline literal)
                {
                    run.Add(literal);
                    continue;
                }

                RewriteRun(run, custom, baseUrl);
                run.Clear();

                if (child is ContainerInline nested and not LinkInline)
                {
                    RewriteContainer(nested, custom, baseUrl);
                }
            }

            RewriteRun(run, custom, baseUrl);
        }

        private static void RewriteRun(List<LiteralInline> run, Dictionary<string, CustomEmojiDto> custom, string baseUrl)
        {
            if (run.Count == 0)
            {
                return;
            }

            var replacement = Scan(string.Concat(run.Select(literal => literal.Content.ToString())), custom, baseUrl);
            if (replacement == null)
            {
                return;
            }

            foreach (var inline in replacement)
            {
                run[0].InsertBefore(inline);
            }

            foreach (var literal in run)
            {
                literal.Remove();
            }
        }

        private static List<Inline> Scan(string text, Dictionary<string, CustomEmojiDto> custom, string baseUrl)
        {
            List<Inline> output = null;
            var last = 0;
            var boundary = -1;
            var start = 0;

            while (start < text.Length)
            {
                var match = Token.Match(text, start);
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

                StandardEmojiTable.TryGet(name, out var variants);
                var tone = 0;
                if (variants?.Count == ToneVariantCount)
                {
                    var toneMatch = Tone.Match(text, end);
                    if (toneMatch.Success)
                    {
                        tone = toneMatch.Groups[1].Value[0] - '0';
                        end += toneMatch.Length;
                    }
                }

                if (end < text.Length && IsWordAt(text, end))
                {
                    start = match.Index + 1;
                    continue;
                }

                var emoji = Render(name, tone, variants, custom, baseUrl);
                if (emoji.Count > 0)
                {
                    output ??= new List<Inline>();
                    if (match.Index > last)
                    {
                        output.Add(new LiteralInline(text[last..match.Index]));
                    }

                    output.AddRange(emoji);
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

        private static List<Inline> Render(string name, int tone, IReadOnlyList<string> variants, Dictionary<string, CustomEmojiDto> custom, string baseUrl)
        {
            if (custom.TryGetValue(name, out var emoji))
            {
                var inlines = new List<Inline> { new HtmlInline(ImageTag(emoji, baseUrl)) };
                if (tone > 0)
                {
                    inlines.Add(new LiteralInline($":skin-tone-{tone}:"));
                }

                return inlines;
            }

            if (variants == null)
            {
                return new List<Inline>();
            }

            return new List<Inline> { new LiteralInline(variants[tone > 0 ? tone - 1 : 0]) };
        }

        private static string ImageTag(CustomEmojiDto emoji, string baseUrl)
        {
            var src = WebUtility.HtmlEncode(baseUrl + emoji.Url);
            var label = WebUtility.HtmlEncode($":{emoji.Name}:");

            return $"<img src=\"{src}\" alt=\"{label}\" title=\"{label}\" width=\"20\" height=\"20\" style=\"vertical-align:-4px\">";
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
