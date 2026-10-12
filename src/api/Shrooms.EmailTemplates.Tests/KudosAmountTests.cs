using NUnit.Framework;
using Razor.Templating.Core;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;

namespace Shrooms.EmailTemplates.Tests
{
    // Kudos amounts are shown as whole numbers, rounded half away from zero, matching the web client.
    [TestFixture]
    public class KudosAmountTests
    {
        [TestCase(10.92, "11")]
        [TestCase(10.5, "11")]
        [TestCase(10.49, "10")]
        public async Task Received_WithCents_ShowsTheRoundedAmount(decimal amount, string expected)
        {
            var html = await RazorTemplateEngine.RenderAsync(EmailTemplateCacheKeys.KudosReceived, Model(amount));

            Assert.Multiple(() =>
            {
                Assert.That(html, Does.Contain($"You received {expected} kudos for "));
                Assert.That(html, Does.Not.Contain(amount.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            });
        }

        [Test]
        public async Task Decreased_WithCents_ShowsTheRoundedAmount()
        {
            var html = await RazorTemplateEngine.RenderAsync(EmailTemplateCacheKeys.KudosDecreased, Model(10.92m));

            Assert.That(html, Does.Contain("Your kudos were reduced by 11."));
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
