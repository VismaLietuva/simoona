using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Shrooms.EmailTemplates.Tests
{
    // HeaderFooter.cshtml is the mjml output with the edits Design/README.md lists. Recompiling
    // mjml is deliberately not a CI step, but keeping the two in step can be - so "edit the
    // .mjml, not this file" is enforced rather than merely asked for.
    [TestFixture]
    public class LayoutIsGeneratedTests
    {
        private static readonly string LayoutRoot = Path.Combine(TestContext.CurrentContext.TestDirectory, "Layout");

        // The content slot is a full <tr> in the mjml output and a bare @RenderBody() in the Razor
        // layout, so it is matched as one block. Written pre-collapsed, like both sides of the compare.
        private const string ContentSlotRow =
            "<tr> <td align=\"left\" style=\"font-size:0px;padding:0;word-break:break-word;\"> "
            + "<div style=\"font-family:Inter,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;"
            + "font-size:14px;line-height:1.6;text-align:left;color:#0a0a0a;\">CONTENT_SLOT</div> "
            + "</td> </tr>";

        [Test]
        public async Task HeaderFooter_IsTheCompiledMjml()
        {
            var expected = Collapse(await File.ReadAllTextAsync(Path.Combine(LayoutRoot, "layout.html")))
                .Replace(ContentSlotRow, "@RenderBody()")
                .Replace("SETTINGS_URL_SLOT", "@Model.UserNotificationSettingsUrl")
                .Replace("HOME_URL_SLOT", "@Model.HomeUrl");

            // Undo the blanket @ escaping instead of listing which at-rules mjml emitted.
            var actual = Collapse(await File.ReadAllTextAsync(Path.Combine(LayoutRoot, "HeaderFooter.cshtml")))
                .Replace("@model Shrooms.Contracts.DataTransferObjects.BaseEmailTemplateViewModel", string.Empty)
                .Replace("@* Generated from Design/layout.mjml - edit the .mjml, not this file. *@", string.Empty)
                .Replace("@@", "@")
                .Trim();

            Assert.That(
                actual,
                Is.EqualTo(expected),
                "HeaderFooter.cshtml no longer matches Design/layout.html. Recompile the mjml and fold it in "
                    + "as Design/README.md describes, rather than editing the .cshtml.");
        }

        private static string Collapse(string html)
        {
            return Regex.Replace(html, @"\s+", " ").Trim();
        }
    }
}
