using NUnit.Framework;
using Razor.Templating.Core;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;

namespace Shrooms.EmailTemplates.Tests
{
    // Automatic awards have no sender, and a deleted sender's name can't be looked up.
    [TestFixture]
    public class KudosSenderTests
    {
        [TestCase(null)]
        [TestCase("")]
        public async Task Received_WithoutASender_DoesNotNameOne(string sender)
        {
            var html = await RazorTemplateEngine.RenderAsync(EmailTemplateCacheKeys.KudosReceived, Model(sender));

            Assert.Multiple(() =>
            {
                Assert.That(html, Does.Contain("for \"Committee Membership\"."));
                Assert.That(html, Does.Not.Contain(", from"));
                Assert.That(html, Does.Contain("You received 10 kudos for "));
            });
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task Decreased_WithoutASender_DoesNotNameOne(string sender)
        {
            var html = await RazorTemplateEngine.RenderAsync(EmailTemplateCacheKeys.KudosDecreased, Model(sender));

            Assert.Multiple(() =>
            {
                Assert.That(html, Does.Not.Contain(" by <strong>"));
                Assert.That(html, Does.Contain("Your kudos were reduced by 10."));
            });
        }

        private static KudosReceivedDecreasedEmailTemplateViewModel Model(string sender) =>
            new KudosReceivedDecreasedEmailTemplateViewModel(
                "https://x/settings",
                10,
                "Committee Membership",
                sender,
                "For your valuable contribution to the Academic Committee!",
                "https://x/kudos");
    }
}
