namespace Shrooms.Domain.Helpers
{
    public interface IMarkdownConverter
    {
        string ConvertToHtml(string markdown);
    }
}