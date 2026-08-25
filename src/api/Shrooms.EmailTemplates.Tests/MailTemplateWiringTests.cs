using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using Razor.Templating.Core;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Infrastructure.Email.Templating;

namespace Shrooms.EmailTemplates.Tests
{
    // Covers the DI path the golden-file tests bypass.
    [TestFixture]
    public class MailTemplateWiringTests
    {
        [Test]
        public async Task MailTemplate_ResolvedFromContainer_RendersByCacheKey()
        {
            var services = new ServiceCollection();
            services.AddRazorTemplating();
            var provider = services.BuildServiceProvider();

            var appSettings = Substitute.For<IApplicationSettings>();
            appSettings.ClientUrl.Returns("https://app.simoona.example.com/");

            var sut = new MailTemplate(provider.GetRequiredService<IRazorTemplateEngine>(), appSettings);
            var viewModel = new KudosSentEmailTemplateViewModel(
                "https://simoona.example.com/settings", "Rasa Petraitiene", 25, "Thanks!", "https://simoona.example.com/kudos");

            var html = await sut.GenerateAsync(viewModel, EmailTemplateCacheKeys.KudosSent);

            Assert.Multiple(() =>
            {
                Assert.That(html, Does.Contain("Rasa Petraitiene"));
                Assert.That(html, Does.Contain("Update your notification preferences"), "layout was not applied");
                Assert.That(html, Does.Contain("https://app.simoona.example.com/"), "home link was not filled in");
            });
        }
    }
}
