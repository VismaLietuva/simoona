using Ganss.Xss;

namespace Shrooms.Domain.Helpers
{
    public static class HtmlSanitizerHelper
    {
        private static readonly HtmlSanitizer _sanitizer = CreateSanitizer();

        public static string Sanitize(string html) => _sanitizer.Sanitize(html);

        private static HtmlSanitizer CreateSanitizer()
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedSchemes.Add("mailto");
            return sanitizer;
        }
    }
}