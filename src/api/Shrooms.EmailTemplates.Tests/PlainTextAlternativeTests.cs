using NUnit.Framework;
using Shrooms.Infrastructure.Email;

namespace Shrooms.EmailTemplates.Tests
{
    // The text/plain alternative is derived from the rendered html, so it is worth checking against
    // a real render rather than a hand-written snippet.
    [TestFixture]
    public class PlainTextAlternativeTests
    {
        private static readonly string GoldenRoot = Path.Combine(TestContext.CurrentContext.TestDirectory, "GoldenFiles");

        [Test]
        public async Task Convert_OnARenderedEmail_KeepsTheCopyAndDropsTheChrome()
        {
            var html = await File.ReadAllTextAsync(Path.Combine(GoldenRoot, "Kudos.KudosReceived.html"));

            var text = HtmlToPlainTextConverter.Convert(html);

            TestContext.Out.WriteLine(text);
            Assert.Multiple(() =>
            {
                Assert.That(text, Does.Not.Contain("<"), "markup survived");
                Assert.That(text, Does.Not.Contain("&amp;"), "entities were not decoded");
                Assert.That(text, Does.Not.Contain("mso"), "conditional comments survived");
                Assert.That(text, Does.Not.Contain("font-family"), "css survived");
                Assert.That(text, Does.Contain("S/moona"), "wordmark is missing");
                Assert.That(text, Does.Contain("Rasa Petraitiene"), "body copy is missing");
                Assert.That(text, Does.Contain("You received 25 kudos"), "the headline is missing");
                Assert.That(text, Does.Contain("Update your notification preferences"), "footer is missing");
                Assert.That(text, Does.Contain("https://simoona.example.com/kudos"), "the cta url is missing");
                Assert.That(text, Does.Not.Match(@"\n[ \t]*\n[ \t]*\n"), "blank runs were not collapsed");
            });
        }

        // A template that renders markup into a Razor-encoded slot shows the tags as literal text,
        // and the text part faithfully repeats them. Catching that here beats reading 32 goldens.
        [Test]
        public void Convert_OnEveryRenderedEmail_LeavesNoLiteralMarkup()
        {
            var offenders = Directory.EnumerateFiles(GoldenRoot, "*.html")
                .Select(path => new { Name = Path.GetFileName(path), Text = HtmlToPlainTextConverter.Convert(File.ReadAllText(path)) })
                .Where(x => x.Text.Contains('<') || x.Text.Contains('>'))
                .Select(x => x.Name)
                .ToArray();

            Assert.That(offenders, Is.Empty, "a seed is feeding markup into an encoded slot");
        }

        [Test]
        public void Convert_OnEscapedAngleBracketInText_KeepsIt()
        {
            var text = HtmlToPlainTextConverter.Convert("<p>5 &lt; 6 and 7 &gt; 6</p>");

            Assert.That(text, Is.EqualTo("5 < 6 and 7 > 6"));
        }

        [Test]
        public void Convert_OnHiddenPreheader_DropsIt()
        {
            var html = "<div style=\"display:none;font-size:1px;\">Hidden summary</div><p>Real copy</p>";

            var text = HtmlToPlainTextConverter.Convert(html);

            Assert.That(text, Is.EqualTo("Real copy"));
        }

        [Test]
        public void Convert_OnAnchor_KeepsLabelAndUrl()
        {
            var text = HtmlToPlainTextConverter.Convert("<a href=\"https://example.com/x\">Open the event</a>");

            Assert.That(text, Is.EqualTo("Open the event (https://example.com/x)"));
        }
    }
}
