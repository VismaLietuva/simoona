using NUnit.Framework;
using Shrooms.Domain.Helpers;

namespace Shrooms.Tests.Helpers
{
    [TestFixture]
    public class MarkdigMarkdownConverterTests
    {
        private MarkdigMarkdownConverter _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new MarkdigMarkdownConverter();
        }

        [Test]
        public void ConvertToHtml_Bold_RendersStrongTag()
        {
            var result = _sut.ConvertToHtml("**bold**");

            Assert.That(result, Is.EqualTo("<p><strong>bold</strong></p>\n"));
        }

        [Test]
        public void ConvertToHtml_Italic_RendersEmTag()
        {
            var result = _sut.ConvertToHtml("*italic*");

            Assert.That(result, Is.EqualTo("<p><em>italic</em></p>\n"));
        }

        [Test]
        public void ConvertToHtml_Strikethrough_RendersDelTag()
        {
            // Verifies the UseEmphasisExtras(Strikethrough) extension is active
            var result = _sut.ConvertToHtml("~~strikethrough~~");

            Assert.That(result, Is.EqualTo("<p><del>strikethrough</del></p>\n"));
        }

        [Test]
        public void ConvertToHtml_SoftLineBreak_RendersHardLineBreak()
        {
            // Verifies the UseSoftlineBreakAsHardlineBreak extension is active.
            // Standard CommonMark treats a single newline as a space; this extension renders it as <br />.
            var result = _sut.ConvertToHtml("line1\nline2");

            Assert.That(result, Is.EqualTo("<p>line1<br />\nline2</p>\n"));
        }

        [Test]
        public void ConvertToHtml_EmptyString_ReturnsEmpty()
        {
            var result = _sut.ConvertToHtml(string.Empty);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ConvertToHtml_Heading_RendersHeadingTag()
        {
            var result = _sut.ConvertToHtml("# Hello");

            Assert.That(result, Is.EqualTo("<h1>Hello</h1>\n"));
        }
    }
}
