using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;

namespace Shrooms.Infrastructure.Email
{
    public class MailingService : IMailingService
    {
        private readonly EmailBuildingStrategy _emailBuildingStrategy;
        private readonly IMailSendingService _mailSendingService;
        private readonly TelemetryClient _telemetryClient;

        public MailingService(IMailSendingService mailSendingService, IApplicationSettings appSettings, TelemetryClient telemetryClient)
        {
            _mailSendingService = mailSendingService;
            _telemetryClient = telemetryClient;
            _emailBuildingStrategy = appSettings.EmailBuildingStrategy;
        }

        public async Task SendEmailAsync(EmailDto email, bool skipDomainChange = false)
        {
            await SendEmailInternalAsync(email, skipDomainChange);
        }

        public async Task SendEmailsAsync(IEnumerable<EmailDto> emails, bool skipDomainChange = false)
        {
            foreach (EmailDto email in emails)
            {
                await SendEmailAsync(email, skipDomainChange);
            }
        }

        private async Task SendEmailInternalAsync(EmailDto email, bool skipDomainChange = false)
        {
            if (!_mailSendingService.IsMailSenderConfigured())
            {
                return;
            }

            if (!email.Receivers.Any())
            {
                return;
            }

            try
            {
                IEnumerable<MailMessage> messages = BuildMessages(email, skipDomainChange);
                await _mailSendingService.SendAsync(messages);
            }
            catch (SmtpException ex)
            {
                LogSendFailure(ex);
            }
        }

        private string ChangeEmailDomain(string senderEmail, string senderFullName)
        {
            var mailAddress = new MailAddress(senderEmail);
            return $"{senderFullName} <{mailAddress.User}@simoona.com>";
        }

        private IEnumerable<MailMessage> BuildMessages(EmailDto email, bool skipDomainChange = false)
        {
            switch (_emailBuildingStrategy)
            {
                case EmailBuildingStrategy.SingleTo:
                    foreach (string emailReceiver in email.Receivers)
                    {
                        yield return BuildMessage(
                            email with { Receivers = new[] { emailReceiver } },
                            skipDomainChange,
                            recipientsTo: true);
                    }
                    break;
                default:
                case EmailBuildingStrategy.AllTo:
                    yield return BuildMessage(email, skipDomainChange, recipientsTo: true);
                    break;
                case EmailBuildingStrategy.AllBcc:
                    yield return BuildMessage(email, skipDomainChange, recipientsTo: false);
                    break;
            }
        }

        private MailMessage BuildMessage(EmailDto email, bool skipDomainChange, bool recipientsTo)
        {
            var mailMessage = new MailMessage();

            var sender = skipDomainChange
                ? $"{email.SenderFullName} <{email.SenderEmail}>"
                : ChangeEmailDomain(email.SenderEmail, email.SenderFullName);

            mailMessage.From = new MailAddress(sender);

            if (recipientsTo)
            {
                foreach (var receiver in email.Receivers)
                {
                    mailMessage.To.Add(receiver);
                }
            }
            else
            {
                mailMessage.To.Add(sender);
                foreach (var receiver in email.Receivers)
                {
                    mailMessage.Bcc.Add(receiver);
                }
            }

            foreach (var attachment in email.Attachments)
            {
                mailMessage.Attachments.Add(attachment);
            }

            mailMessage.Subject = email.Subject;

            if (IsRenderedTemplate(email.Body))
            {
                // multipart/alternative: text first, html last - clients take the last part they support.
                mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                    HtmlToPlainTextConverter.Convert(email.Body), null, MediaTypeNames.Text.Plain));
                mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                    email.Body, null, MediaTypeNames.Text.Html));
            }
            else
            {
                // Support requests and the like pass the sender's own text straight through. Deriving
                // a text part from it would collapse the line breaks they typed, and a null body has
                // always been allowed here.
                mailMessage.Body = email.Body;
                mailMessage.IsBodyHtml = true;
            }

            return mailMessage;
        }

        // Only the Razor templates produce a full document; everything else is raw text.
        private static bool IsRenderedTemplate(string body)
        {
            return body != null && body.Contains("<html", StringComparison.OrdinalIgnoreCase);
        }

        private void LogSendFailure(SmtpException ex)
        {
            var exceptionTelemetry = new ExceptionTelemetry
            {
                Exception = ex,
                Message = "Failed to send message"
            };

            _telemetryClient.TrackException(exceptionTelemetry);
        }
    }
}
