using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects.Models.Emoji;
using Shrooms.Domain.Helpers;

namespace Shrooms.Tests.Helpers
{
    [TestFixture]
    public class MarkdigMarkdownConverterTests
    {
        private const string ParrotImg =
            "<img src=\"https://api.example.com/storage/org/parrot.gif\" alt=\":parrot:\" title=\":parrot:\" width=\"20\" height=\"20\" style=\"vertical-align:-4px\">";

        private static readonly CustomEmojiDto Parrot = new CustomEmojiDto { Id = 1, Name = "parrot", Url = "/storage/org/parrot.gif" };

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
        public void ConvertToHtml_Null_ReturnsEmpty()
        {
            var result = _sut.ConvertToHtml(null);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ConvertToHtml_WhitespaceOnly_ReturnsEmpty()
        {
            var result = _sut.ConvertToHtml("   ");

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ConvertToHtml_TabsOnly_ReturnsEmpty()
        {
            var result = _sut.ConvertToHtml("\t\t");

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ConvertToHtml_MixedWhitespace_ReturnsEmpty()
        {
            var result = _sut.ConvertToHtml(" \t \n ");

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ConvertToHtml_Heading_RendersHeadingTag()
        {
            var result = _sut.ConvertToHtml("# Hello");

            Assert.That(result, Is.EqualTo("<h1>Hello</h1>\n"));
        }

        private string Convert(string markdown, params CustomEmojiDto[] emojis)
        {
            return _sut.ConvertToHtml(markdown, emojis, "https://api.example.com/");
        }

        [Test]
        public void ConvertToHtml_CustomEmoji_RendersImage()
        {
            Assert.That(Convert("hi :parrot:", Parrot), Is.EqualTo($"<p>hi {ParrotImg}</p>\n"));
        }

        [Test]
        public void ConvertToHtml_StandardEmoji_RendersUnicode()
        {
            Assert.That(Convert(":tada:"), Is.EqualTo("<p>🎉</p>\n"));
        }

        [Test]
        public void ConvertToHtml_CustomWinsOverStandardName()
        {
            var tada = new CustomEmojiDto { Name = "tada", Url = "/storage/org/t.png" };

            Assert.That(
                Convert(":tada:", tada),
                Is.EqualTo("<p><img src=\"https://api.example.com/storage/org/t.png\" alt=\":tada:\" title=\":tada:\" width=\"20\" height=\"20\" style=\"vertical-align:-4px\"></p>\n"));
        }

        [Test]
        public void ConvertToHtml_NameWithUnderscores_RendersUnicode()
        {
            Assert.That(Convert("ok :white_check_mark: and :heart_eyes:"), Is.EqualTo("<p>ok ✅ and 😍</p>\n"));
        }

        [Test]
        public void ConvertToHtml_UnknownThenKnown_RendersTheKnownOne()
        {
            Assert.That(Convert(":nope::tada:"), Is.EqualTo("<p>:nope:🎉</p>\n"));
        }

        [Test]
        public void ConvertToHtml_UnknownName_StaysLiteral()
        {
            Assert.That(Convert(":nope:"), Is.EqualTo("<p>:nope:</p>\n"));
        }

        [Test]
        public void ConvertToHtml_GluedBefore_StaysLiteral()
        {
            Assert.That(Convert("a:tada:"), Is.EqualTo("<p>a:tada:</p>\n"));
        }

        [Test]
        public void ConvertToHtml_GluedAfter_StaysLiteral()
        {
            Assert.That(Convert(":tada:b"), Is.EqualTo("<p>:tada:b</p>\n"));
        }

        [Test]
        public void ConvertToHtml_AdjacentTokens_RenderBoth()
        {
            Assert.That(Convert(":tada::tada:"), Is.EqualTo("<p>🎉🎉</p>\n"));
        }

        [Test]
        public void ConvertToHtml_Uppercase_StaysLiteral()
        {
            Assert.That(Convert(":Tada:"), Is.EqualTo("<p>:Tada:</p>\n"));
        }

        [Test]
        public void ConvertToHtml_ToneOnToneCapable_RendersTonedCharacter()
        {
            Assert.That(Convert(":+1::skin-tone-3:"), Is.EqualTo("<p>👍🏼</p>\n"));
        }

        [Test]
        public void ConvertToHtml_ToneOnCustom_KeepsToneText()
        {
            Assert.That(Convert(":parrot::skin-tone-3:", Parrot), Is.EqualTo($"<p>{ParrotImg}:skin-tone-3:</p>\n"));
        }

        [Test]
        public void ConvertToHtml_InsideInlineCode_Untouched()
        {
            Assert.That(Convert("`:tada:`"), Is.EqualTo("<p><code>:tada:</code></p>\n"));
        }

        [Test]
        public void ConvertToHtml_InsideLinkLabel_Untouched()
        {
            Assert.That(Convert("[:tada:](https://x.y)"), Is.EqualTo("<p><a href=\"https://x.y\">:tada:</a></p>\n"));
        }

        [Test]
        public void ConvertToHtml_InsideEmphasis_Rendered()
        {
            Assert.That(Convert("**:tada:**"), Is.EqualTo("<p><strong>🎉</strong></p>\n"));
        }

        [Test]
        public void ConvertToHtml_InsideHeading_Rendered()
        {
            Assert.That(Convert("# :tada:"), Is.EqualTo("<h1>🎉</h1>\n"));
        }

        [Test]
        public void ConvertToHtml_NullListAndNullBaseUrl_DoesNotThrow()
        {
            Assert.That(_sut.ConvertToHtml(":tada: :partyparrot:", null, null), Is.EqualTo("<p>🎉 :partyparrot:</p>\n"));
        }

        [Test]
        public void ConvertToHtml_NullBaseUrl_UsesRelativeUrl()
        {
            Assert.That(
                _sut.ConvertToHtml(":parrot:", new[] { Parrot }, null),
                Is.EqualTo("<p><img src=\"/storage/org/parrot.gif\" alt=\":parrot:\" title=\":parrot:\" width=\"20\" height=\"20\" style=\"vertical-align:-4px\"></p>\n"));
        }

        [Test]
        public void ConvertToHtml_OneArgumentOverload_LeavesEmojiAlone()
        {
            Assert.That(_sut.ConvertToHtml(":tada:"), Is.EqualTo("<p>:tada:</p>\n"));
        }
    }
}
