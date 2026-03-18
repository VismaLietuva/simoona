using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using MimeKit;
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
                IEnumerable<MimeMessage> messages = BuildMessages(email, skipDomainChange);
                await _mailSendingService.SendAsync(messages);
            }
            catch (SmtpCommandException ex)
            {
                LogSendFailure(ex);
            }
        }

        private string ChangeEmailDomain(string senderEmail, string senderFullName)
        {
            var mailAddress = MailboxAddress.Parse(senderEmail);
            return $"{senderFullName} <{mailAddress.Address.Split('@')[0]}@simoona.com>";
        }

        private IEnumerable<MimeMessage> BuildMessages(EmailDto email, bool skipDomainChange = false)
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

        private MimeMessage BuildMessage(EmailDto email, bool skipDomainChange, bool recipientsTo)
        {
            var mimeMessage = new MimeMessage();

            var sender = skipDomainChange
                ? $"{email.SenderFullName} <{email.SenderEmail}>"
                : ChangeEmailDomain(email.SenderEmail, email.SenderFullName);

            mimeMessage.From.Add(MailboxAddress.Parse(sender));

            if (recipientsTo)
            {
                foreach (var receiver in email.Receivers)
                {
                    mimeMessage.To.Add(MailboxAddress.Parse(receiver));
                }
            }
            else
            {
                mimeMessage.To.Add(MailboxAddress.Parse(sender));
                foreach (var receiver in email.Receivers)
                {
                    mimeMessage.Bcc.Add(MailboxAddress.Parse(receiver));
                }
            }

            mimeMessage.Subject = email.Subject;

            var builder = new BodyBuilder { HtmlBody = email.Body };
            if (email.Attachment != null)
            {
                builder.Attachments.Add(email.Attachment);
            }

            mimeMessage.Body = builder.ToMessageBody();

            return mimeMessage;
        }

        private void LogSendFailure(SmtpCommandException ex)
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
