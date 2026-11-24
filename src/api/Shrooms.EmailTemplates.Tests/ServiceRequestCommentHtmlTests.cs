using NUnit.Framework;
using Razor.Templating.Core;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;

namespace Shrooms.EmailTemplates.Tests
{
    [TestFixture]
    public class ServiceRequestCommentHtmlTests
    {
        private const string RenderedComment = "<p><strong>On</strong> the way</p><img src=\"x\" onerror=\"alert(1)\">";

        [Test]
        public async Task RendersTheCommentAsSanitizedHtml()
        {
            var model = new ServiceRequestCommentEmailTemplateViewModel(
                "https://x/settings", "Broken monitor", "Rasa", RenderedComment, "https://x/service-requests/1");

            var html = await RazorTemplateEngine.RenderAsync(EmailPremiumTemplateCacheKeys.ServiceRequestComment, model);

            Assert.Multiple(() =>
            {
                Assert.That(html, Does.Contain("<strong>On</strong> the way"));
                Assert.That(html, Does.Not.Contain("onerror"));
                Assert.That(html, Does.Contain("class=\"simoona-rich-text\""));
            });
        }
    }
}
