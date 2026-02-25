using Ganss.Xss;

namespace Shrooms.Domain.Helpers
{
    public static class HtmlSanitizerHelper
    {
        private static readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

        public static string Sanitize(string html) => _sanitizer.Sanitize(html);
    }
}