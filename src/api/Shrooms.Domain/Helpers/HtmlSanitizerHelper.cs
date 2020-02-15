using Ganss.XSS;

namespace Shrooms.Domain.Helpers
{
    public static class HtmlSanitizerHelper
    {
        private static readonly IHtmlSanitizer _sanitizer = new HtmlSanitizer();

        public static string Sanitize(string html) => _sanitizer.Sanitize(html);
    }
}