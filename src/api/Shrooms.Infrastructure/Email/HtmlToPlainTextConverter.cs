using System.Net;
using System.Text.RegularExpressions;

namespace Shrooms.Infrastructure.Email
{
    // Derives the text/plain alternative from the rendered HTML. Targeted at our own generated
    // markup rather than arbitrary HTML - the templates are the only input.
    public static class HtmlToPlainTextConverter
    {
        // Block ends become this first, so the markup's own newlines and indentation can collapse to
        // spaces without splitting a sentence across lines.
        private const string Sentinel = "\u0000";

        private static readonly Regex Dropped = new(
            @"<head\b[^>]*>.*?</head>|<(style|script)\b[^>]*>.*?</\1>|<!--.*?-->|<div[^>]*display:\s*none[^>]*>.*?</div>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex Links = new(
            @"<a\b[^>]*?href\s*=\s*[""']([^""']*)[""'][^>]*>(.*?)</a>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex Breaks = new(
            @"<br\s*/?>|</(p|tr|td|div|h1|h2|h3|li)\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex Tags = new("<[^>]+>", RegexOptions.Compiled);

        private static readonly Regex Whitespace = new(@"[^\S\u0000]+", RegexOptions.Compiled);

        private static readonly Regex BlankRun = new(@"\n{3,}", RegexOptions.Compiled);

        public static string Convert(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var text = Dropped.Replace(html, string.Empty);

            // A bare URL is more use in a text part than an anchor's label alone.
            text = Links.Replace(text, match =>
            {
                var label = Tags.Replace(match.Groups[2].Value, string.Empty).Trim();
                var href = match.Groups[1].Value.Trim();
                if (string.IsNullOrEmpty(href) || label.Equals(href, StringComparison.OrdinalIgnoreCase))
                {
                    return label;
                }

                return string.IsNullOrEmpty(label) ? href : $"{label} ({href})";
            });

            text = Breaks.Replace(text, Sentinel);

            // Strip, then decode - not the reverse. Decoding first would turn an escaped "5 &lt; 6"
            // into a tag for the stripper to eat. The cost is that markup which the html part shows
            // as literal text shows the same way here, which is faithful: the two parts agree.
            text = Tags.Replace(text, string.Empty);
            text = WebUtility.HtmlDecode(text);
            text = Whitespace.Replace(text, " ");

            var lines = text.Split(Sentinel).Select(line => line.Trim());
            return BlankRun.Replace(string.Join("\n", lines), "\n\n").Trim();
        }
    }
}
