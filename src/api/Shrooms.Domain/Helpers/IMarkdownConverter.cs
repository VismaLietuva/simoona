namespace Shrooms.Domain.Helpers
{
    public interface IMarkdownConverter
    {
        string ConvertToHtml(string markdown);

        string ConvertToHtml(string markdown, IReadOnlyDictionary<string, string> customEmojiImages);
    }
}
