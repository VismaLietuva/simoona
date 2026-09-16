using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NUnit.Framework;
using Razor.Templating.Core;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.EmailTemplates.TagHelpers;

namespace Shrooms.EmailTemplates.Tests
{
    // These helpers build their markup as strings and emit it with SetHtmlContent, which opts out
    // of Razor's automatic encoding. Child content arrives already encoded; attribute-bound
    // properties do not, so each one has to encode its own.
    [TestFixture]
    public class TagHelperEncodingTests
    {
        [Test]
        public async Task Button_Href_CannotEscapeTheAttribute()
        {
            var html = await RenderAsync(
                new EmailButtonTagHelper { Href = "https://x/kudos\" onclick=\"alert(1)" }, "Open");

            Assert.Multiple(() =>
            {
                // The quote must stay inside the value rather than closing it and starting an attribute.
                Assert.That(html, Does.Not.Contain("\" onclick"));
                Assert.That(html, Does.Contain("&quot; onclick=&quot;"));
            });
        }

        [Test]
        public async Task Button_Href_KeepsQueryStringsUsable()
        {
            var html = await RenderAsync(new EmailButtonTagHelper { Href = "https://x/k?a=1&b=2" }, "Open");

            // &amp; is the correct spelling inside an attribute; clients decode it back.
            Assert.That(html, Does.Contain("href=\"https://x/k?a=1&amp;b=2\""));
        }

        [Test]
        public async Task Detail_Label_CannotInjectMarkup()
        {
            var html = await RenderAsync(new EmailDetailTagHelper { Label = "<script>alert(1)</script>" }, "Vilnius");

            Assert.That(html, Does.Not.Contain("<script>"));
        }

        [Test]
        public async Task Button_RenderedThroughATemplate_EncodesTheModelUrl()
        {
            var viewModel = new KudosSentEmailTemplateViewModel(
                "https://x/settings", "Rasa", 25, "Thanks!", "https://x/kudos\" onclick=\"alert(1)");

            var html = await RazorTemplateEngine.RenderAsync(EmailTemplateCacheKeys.KudosSent, viewModel);

            Assert.That(html, Does.Not.Contain("\" onclick"));
        }

        private static async Task<string> RenderAsync(TagHelper helper, string childContent)
        {
            var context = new TagHelperContext(
                new TagHelperAttributeList(), new Dictionary<object, object>(), "test");
            var output = new TagHelperOutput(
                "email-tag",
                new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(
                    new DefaultTagHelperContent().SetHtmlContent(childContent)));

            await helper.ProcessAsync(context, output);

            await using var writer = new StringWriter();
            output.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }
    }
}
