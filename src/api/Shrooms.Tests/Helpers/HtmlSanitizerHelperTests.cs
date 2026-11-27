using NUnit.Framework;
using Shrooms.Domain.Helpers;

namespace Shrooms.Tests.Helpers
{
    [TestFixture]
    public class HtmlSanitizerHelperTests
    {
        [Test]
        public void Sanitize_KeepsMailtoLinks()
        {
            var html = HtmlSanitizerHelper.Sanitize("<a href=\"mailto:jonas@example.com\">jonas@example.com</a>");

            Assert.That(html, Is.EqualTo("<a href=\"mailto:jonas@example.com\">jonas@example.com</a>"));
        }

        [TestCase("javascript:alert(1)")]
        [TestCase("tel:+37060000000")]
        [TestCase("ftp://files.example.com/x")]
        public void Sanitize_DropsLinksWithOtherSchemes(string href)
        {
            var html = HtmlSanitizerHelper.Sanitize($"<a href=\"{href}\">x</a>");

            Assert.That(html, Is.EqualTo("<a>x</a>"));
        }
    }
}
