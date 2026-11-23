using NUnit.Framework;
using Razor.Templating.Core;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;

namespace Shrooms.EmailTemplates.Tests
{
    [TestFixture]
    public class KudosCommentHtmlTests
    {
        private const string RenderedComment = "<p><strong>Great</strong> job</p><img src=\"x\" onerror=\"alert(1)\">";

        private static IEnumerable<TestCaseData> Templates()
        {
            yield return new TestCaseData(
                EmailTemplateCacheKeys.KudosSent,
                new KudosSentEmailTemplateViewModel("https://x/settings", "Rasa", 5, RenderedComment, "https://x/kudos"))
                .SetName("KudosSent");
            yield return new TestCaseData(
                EmailTemplateCacheKeys.KudosReceived,
                new KudosReceivedDecreasedEmailTemplateViewModel("https://x/settings", 5, "Send", "Rasa", RenderedComment, "https://x/kudos"))
                .SetName("KudosReceived");
            yield return new TestCaseData(
                EmailTemplateCacheKeys.KudosDecreased,
                new KudosReceivedDecreasedEmailTemplateViewModel("https://x/settings", 5, "Minus", "Rasa", RenderedComment, "https://x/kudos"))
                .SetName("KudosDecreased");
            yield return new TestCaseData(
                EmailTemplateCacheKeys.KudosRejected,
                new KudosRejectedEmailTemplateViewModel("https://x/settings", "Ada", 5, "Send", RenderedComment, "No", "https://x/kudos"))
                .SetName("KudosRejected");
            yield return new TestCaseData(
                EmailPremiumTemplateCacheKeys.LoyaltyKudosReceived,
                new LoyaltyKudosReceivedDecreasedEmailTemplateViewModel("https://x/settings", 5, "Loyalty", "Org", RenderedComment, "https://x/kudos"))
                .SetName("LoyaltyKudosReceived");
            yield return new TestCaseData(
                EmailPremiumTemplateCacheKeys.LoyaltyKudosDecreased,
                new LoyaltyKudosReceivedDecreasedEmailTemplateViewModel("https://x/settings", 5, "Loyalty", "Org", RenderedComment, "https://x/kudos"))
                .SetName("LoyaltyKudosDecreased");
        }

        [TestCaseSource(nameof(Templates))]
        public async Task RendersTheCommentAsSanitizedHtml(string templateKey, BaseEmailTemplateViewModel model)
        {
            var html = await RazorTemplateEngine.RenderAsync(templateKey, model);

            Assert.Multiple(() =>
            {
                Assert.That(html, Does.Contain("<strong>Great</strong> job"));
                Assert.That(html, Does.Not.Contain("onerror"));
            });
        }
    }
}
