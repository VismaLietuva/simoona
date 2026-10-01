using NUnit.Framework;
using Razor.Templating.Core;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;

namespace Shrooms.EmailTemplates.Tests
{
    // Kudos amounts are shown as whole numbers, cents truncated, matching the web client.
    [TestFixture]
    public class KudosAmountTests
    {
        [Test]
        public async Task Received_WithCents_ShowsTheTruncatedAmount()
        {
            var html = await RazorTemplateEngine.RenderAsync(EmailTemplateCacheKeys.KudosReceived, Model(10.92m));

            Assert.Multiple(() =>
            {
                Assert.That(html, Does.Contain("You received 10 kudos for "));
                Assert.That(html, Does.Not.Contain("10.92"));
            });
        }

        [Test]
        public async Task Decreased_WithCents_ShowsTheTruncatedAmount()
        {
            var html = await RazorTemplateEngine.RenderAsync(EmailTemplateCacheKeys.KudosDecreased, Model(10.92m));

            Assert.That(html, Does.Contain("Your kudos were reduced by 10."));
        }

        private static KudosReceivedDecreasedEmailTemplateViewModel Model(decimal amount) =>
            new KudosReceivedDecreasedEmailTemplateViewModel(
                "https://x/settings",
                amount,
                "Committee Membership",
                null,
                "For your valuable contribution to the Academic Committee!",
                "https://x/kudos");
    }
}
