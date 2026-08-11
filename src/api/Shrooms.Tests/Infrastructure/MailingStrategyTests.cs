using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Infrastructure.Email;

namespace Shrooms.Tests.Infrastructure
{
    [TestFixture]
    public class MailingStrategyTests
    {
        private readonly string[] _recipients = { "one@one.qqq", "two@two.qqq", "three@three.qqq" };
        private IApplicationSettings _settings;
        private IMailSendingService _smtpService;
        private EmailDto _emailDto;

        [SetUp]
        public void TestInitializer()
        {
            _settings = Substitute.For<IApplicationSettings>();
            _smtpService = Substitute.For<IMailSendingService>();

            _smtpService.IsMailSenderConfigured().Returns(true);

            _emailDto = new EmailDto("sender", "senderemail@yes.no", _recipients, "subject", "body");
        }

        [Test]
        public async Task AllTo_SingleEmailSent()
        {
            // Arrange
            _settings.EmailBuildingStrategy.Returns(EmailBuildingStrategy.AllTo);
            var actualSent = new List<MailMessage>();
            TrackActualSent(actualSent);

            var service = new MailingService(_smtpService, _settings, new TelemetryClient(new TelemetryConfiguration()));

            // Act
            await service.SendEmailAsync(_emailDto);

            // Assert
            Assert.That(actualSent.Count, Is.EqualTo(1));
            Assert.That(_recipients, Is.EqualTo(actualSent[0].To.Select(x => x.Address)));
        }

        [Test]
        public async Task AllBcc_SingleEmailSent()
        {
            // Arrange
            _settings.EmailBuildingStrategy.Returns(EmailBuildingStrategy.AllBcc);
            var actualSent = new List<MailMessage>();
            TrackActualSent(actualSent);

            var service = new MailingService(_smtpService, _settings, new TelemetryClient(new TelemetryConfiguration()));

            // Act
            await service.SendEmailAsync(_emailDto);

            // Assert
            Assert.That(actualSent.Count, Is.EqualTo(1));
            Assert.That(_recipients, Is.EqualTo(actualSent[0].Bcc.Select(x => x.Address)));
        }

        [Test]
        public async Task SingleTo_MultipleEmailsSent()
        {
            // Arrange
            _settings.EmailBuildingStrategy.Returns(EmailBuildingStrategy.SingleTo);
            var actualSent = new List<MailMessage>();
            TrackActualSent(actualSent);

            var service = new MailingService(_smtpService, _settings, new TelemetryClient(new TelemetryConfiguration()));

            // Act
            await service.SendEmailAsync(_emailDto);

            // Assert
            Assert.That(actualSent.Count, Is.EqualTo(3));
            Assert.That(_recipients, Is.EqualTo(actualSent.Select(x => x.To.Single().Address)));
        }

        private void TrackActualSent(List<MailMessage> actualSent)
        {
            _smtpService
                .When(x => x.SendAsync(Arg.Any<IEnumerable<MailMessage>>()))
                .Do(callInfo => actualSent.AddRange(callInfo.Arg<IEnumerable<MailMessage>>()));
        }
    }
}
