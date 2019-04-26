using Ganss.XSS;

namespace Shrooms.Domain.Helpers
{
    public static class HtmlSanitizerHelper
    {
        private static readonly IHtmlSanitizer Sanitizer = new HtmlSanitizer();

        public static string Sanitize(string html) => Sanitizer.Sanitize(html);
    }
}