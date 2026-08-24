using System.Net.Mail;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Infrastructure.Email;

namespace Shrooms.EmailTemplates.Tests
{
    // Only the rendered templates get a text/plain twin. Everything else - support requests above
    // all - is the sender's own text, and deriving a text part from it destroyed what they typed.
    [TestFixture]
    public class MailingServiceBodyTests
    {
        private IMailSendingService _sender;
        private List<MailMessage> _sent;
        private MailingService _sut;

        [SetUp]
        public void SetUp()
        {
            _sent = new List<MailMessage>();
            _sender = Substitute.For<IMailSendingService>();
            _sender.IsMailSenderConfigured().Returns(true);
            _sender.SendAsync(Arg.Do<IEnumerable<MailMessage>>(m => _sent.AddRange(m))).Returns(Task.CompletedTask);

            _sut = new MailingService(_sender, Substitute.For<IApplicationSettings>(), new TelemetryClient(new TelemetryConfiguration()));
        }

        [Test]
        public async Task SendEmail_OnARenderedTemplate_AddsBothParts()
        {
            await _sut.SendEmailAsync(Email("<!doctype html><html><body><p>Hi</p></body></html>"));

            var message = _sent.Single();
            Assert.Multiple(() =>
            {
                Assert.That(message.AlternateViews.Select(v => v.ContentType.MediaType),
                    Is.EqualTo(new[] { "text/plain", "text/html" }), "text must come before html");
                Assert.That(ReadView(message, 0), Is.EqualTo("Hi"));
            });
        }

        [Test]
        public async Task SendEmail_OnRawSenderText_KeepsItVerbatim()
        {
            const string typed = "Line one\nLine two\n\nMy screen shows 5 < 6 and a <thing>.";

            await _sut.SendEmailAsync(Email(typed));

            var message = _sent.Single();
            Assert.Multiple(() =>
            {
                Assert.That(message.AlternateViews, Is.Empty, "raw text must not be converted");
                Assert.That(message.Body, Is.EqualTo(typed), "the sender's line breaks and angle brackets must survive");
            });
        }

        [Test]
        public async Task SendEmail_OnNullBody_DoesNotThrow()
        {
            await _sut.SendEmailAsync(Email(null));

            Assert.That(_sent.Single().Body, Is.Empty);
        }

        private static EmailDto Email(string body) => new(new[] { "someone@simoona.com" }, "Subject", body);

        private static string ReadView(MailMessage message, int index)
        {
            var view = message.AlternateViews[index];
            view.ContentStream.Position = 0;
            return new StreamReader(view.ContentStream).ReadToEnd().Trim();
        }
    }
}
