namespace Shrooms.Domain.Helpers
{
    public interface IMarkdownConverter
    {
        string ConvertToHtml(string markdown);

        // Also renders emoji shortcodes. A name listed in customEmojiImages (shortcode without the
        // colons -> absolute image URL) becomes an inline image; a standard name becomes its Unicode
        // character; anything else stays literal.
        string ConvertToHtml(string markdown, IReadOnlyDictionary<string, string> customEmojiImages);
    }
}
