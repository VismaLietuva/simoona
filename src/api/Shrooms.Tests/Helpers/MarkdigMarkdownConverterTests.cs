using System.Collections.Generic;
using NUnit.Framework;
using Shrooms.Domain.Helpers;

namespace Shrooms.Tests.Helpers
{
    [TestFixture]
    public class MarkdigMarkdownConverterTests
    {
        private const string ParrotImg =
            "<img src=\"https://api.example.com/storage/org/shipit.gif\" alt=\":shipit:\" title=\":shipit:\" width=\"20\" height=\"20\" style=\"vertical-align:-4px\">";

        private static readonly IReadOnlyDictionary<string, string> Parrot = new Dictionary<string, string>
        {
            ["shipit"] = "https://api.example.com/storage/org/shipit.gif"
        };

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

        private string Convert(string markdown, IReadOnlyDictionary<string, string> customEmojiImages = null)
        {
            return _sut.ConvertToHtml(markdown, customEmojiImages ?? new Dictionary<string, string>());
        }

        [Test]
        public void ConvertToHtml_TextWithoutColon_IsLeftToMarkdig()
        {
            Assert.That(Convert("plain **bold** text", Parrot), Is.EqualTo("<p>plain <strong>bold</strong> text</p>\n"));
        }

        [Test]
        public void ConvertToHtml_CustomEmoji_RendersImage()
        {
            Assert.That(Convert("hi :shipit:", Parrot), Is.EqualTo($"<p>hi {ParrotImg}</p>\n"));
        }

        [Test]
        public void ConvertToHtml_StandardEmoji_RendersUnicode()
        {
            Assert.That(Convert(":tada:"), Is.EqualTo("<p>🎉</p>\n"));
        }

        [Test]
        public void ConvertToHtml_CustomWinsOverStandardName_AsOnTheWall()
        {
            var tada = new Dictionary<string, string> { ["tada"] = "https://api.example.com/storage/org/t.png" };

            Assert.That(Convert(":tada:", tada), Does.StartWith("<p><img src=\"https://api.example.com/storage/org/t.png\""));
        }

        [TestCase("done:tada::tada:")]
        [TestCase("12:30::tada:")]
        [TestCase("fe80::a:1")]
        [TestCase("status:ok:200")]
        public void ConvertToHtml_ColonChainGluedToAWord_StaysLiteral(string markdown)
        {
            Assert.That(Convert(markdown), Is.EqualTo($"<p>{markdown}</p>\n"));
        }

        [TestCase("**word**:tada:", "<p><strong>word</strong>:tada:</p>\n")]
        [TestCase("`word`:tada:", "<p><code>word</code>:tada:</p>\n")]
        [TestCase(":tada:**word**", "<p>:tada:<strong>word</strong></p>\n")]
        [TestCase("[:tada:](https://x.y)word", "<p><a href=\"https://x.y\">:tada:</a>word</p>\n")]
        public void ConvertToHtml_GluedToAWordInANeighbouringInline_StaysLiteral(string markdown, string html)
        {
            Assert.That(Convert(markdown), Is.EqualTo(html));
        }

        [TestCase("**word** :tada:", "<p><strong>word</strong> 🎉</p>\n")]
        [TestCase("`word` :tada:", "<p><code>word</code> 🎉</p>\n")]
        [TestCase(":tada:**:tada:**", "<p>🎉<strong>🎉</strong></p>\n")]
        [TestCase("(:tada:)**!**", "<p>(🎉)<strong>!</strong></p>\n")]
        public void ConvertToHtml_SeparatedFromTheNeighbouringInline_Rendered(string markdown, string html)
        {
            Assert.That(Convert(markdown), Is.EqualTo(html));
        }

        [TestCase("**:heart:**:tada:", "<p><strong>❤️</strong>🎉</p>\n")]
        [TestCase(":nope:*:tada:*", "<p>:nope:<em>🎉</em></p>\n")]
        public void ConvertToHtml_RightAfterATokenInANeighbouringInline_Rendered(string markdown, string html)
        {
            Assert.That(Convert(markdown), Is.EqualTo(html));
        }

        [Test]
        public void ConvertToHtml_AfterALineBreak_Rendered()
        {
            Assert.That(Convert("word\n:tada:"), Does.Contain("🎉"));
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
            Assert.That(Convert(":shipit::skin-tone-3:", Parrot), Is.EqualTo($"<p>{ParrotImg}:skin-tone-3:</p>\n"));
        }

        [Test]
        public void ConvertToHtml_InsideInlineCode_Untouched()
        {
            Assert.That(Convert("`:tada:`"), Is.EqualTo("<p><code>:tada:</code></p>\n"));
        }

        [Test]
        public void ConvertToHtml_BareUrlWithToken_BecomesALinkAndStaysLiteral()
        {
            Assert.That(
                Convert("see https://x.y/:tada:/docs now"),
                Is.EqualTo("<p>see <a href=\"https://x.y/:tada:/docs\">https://x.y/:tada:/docs</a> now</p>\n"));
        }

        [Test]
        public void ConvertToHtml_BareWwwAddress_BecomesALink()
        {
            Assert.That(Convert("visit www.example.com today"), Does.Contain("<a href=\"http://www.example.com\">www.example.com</a>"));
        }

        [Test]
        public void ConvertToHtml_InsideLinkLabel_RenderedButNotInTheUrl()
        {
            Assert.That(Convert("[hi :tada:](https://x.y/:tada:)"), Is.EqualTo("<p><a href=\"https://x.y/:tada:\">hi 🎉</a></p>\n"));
        }

        [Test]
        public void ConvertToHtml_InsideAutolink_Untouched()
        {
            Assert.That(Convert("<https://x.y/:tada:>"), Is.EqualTo("<p><a href=\"https://x.y/:tada:\">https://x.y/:tada:</a></p>\n"));
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
        public void ConvertToHtml_NullCustomEmoji_RendersStandardOnly()
        {
            Assert.That(_sut.ConvertToHtml(":tada: :partyparrot:", null), Is.EqualTo("<p>🎉 :partyparrot:</p>\n"));
        }

        [Test]
        public void ConvertToHtml_ImageUrlWithAmpersand_IsAttributeEncoded()
        {
            var signed = new Dictionary<string, string> { ["shipit"] = "https://cdn.example.com/p.gif?a=1&b=2" };

            Assert.That(Convert(":shipit:", signed), Does.Contain("src=\"https://cdn.example.com/p.gif?a=1&amp;b=2\""));
        }

        [Test]
        public void ConvertToHtml_OneArgumentOverload_LeavesEmojiAlone()
        {
            Assert.That(_sut.ConvertToHtml(":tada:"), Is.EqualTo("<p>:tada:</p>\n"));
        }

        [Test]
        public void ConvertToHtml_OneArgumentOverload_LeavesBareUrlsAlone()
        {
            Assert.That(_sut.ConvertToHtml("see https://x.y"), Is.EqualTo("<p>see https://x.y</p>\n"));
        }
    }
}
