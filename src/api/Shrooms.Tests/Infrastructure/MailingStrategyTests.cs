using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Moq;
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
        private Mock<IApplicationSettings> _settings = new ();
        private Mock<IMailSendingService> _smtpService = new ();
        private readonly string[] _recipients = { "one@one.qqq", "two@two.qqq", "three@three.qqq" };
        private EmailDto _emailDto;

        [OneTimeSetUp]
        public void SetupCommonMocks()
        {
            _smtpService
                .Setup(x => x.IsMailSenderConfigured())
                .Returns(true);

            _emailDto = new EmailDto("sender", "senderemail@yes.no", _recipients, "subject", "body");
        }

        [Test]
        public async Task AllTo_SingleEmailSent()
        {
            // Arrange
            _settings.SetupGet(x => x.EmailBuildingStrategy).Returns(EmailBuildingStrategy.AllTo);
            var actualSent = new List<MailMessage>();
            TrackActualSent(actualSent);

            var service = new MailingService(_smtpService.Object, _settings.Object);

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
            _settings.SetupGet(x => x.EmailBuildingStrategy).Returns(EmailBuildingStrategy.AllBcc);
            var actualSent = new List<MailMessage>();
            TrackActualSent(actualSent);

            var service = new MailingService(_smtpService.Object, _settings.Object);

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
            _settings.SetupGet(x => x.EmailBuildingStrategy).Returns(EmailBuildingStrategy.SingleTo);
            var actualSent = new List<MailMessage>();
            TrackActualSent(actualSent);

            var service = new MailingService(_smtpService.Object, _settings.Object);

            // Act
            await service.SendEmailAsync(_emailDto);

            // Assert
            Assert.That(actualSent.Count, Is.EqualTo(3));
            Assert.That(_recipients, Is.EqualTo(actualSent.Select(x => x.To.Single().Address)));
        }

        private void TrackActualSent(List<MailMessage> actualSent)
        {
            _smtpService
                .Setup(x => x.SendAsync(It.IsAny<IEnumerable<MailMessage>>()))
                .Callback((IEnumerable<MailMessage> messages) => actualSent.AddRange(messages))
                .Returns(Task.CompletedTask);
        }
    }
}
