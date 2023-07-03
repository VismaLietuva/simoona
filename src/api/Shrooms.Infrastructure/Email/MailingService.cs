using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNet.Identity;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;

namespace Shrooms.Infrastructure.Email
{
    public class MailingService : IMailingService, IIdentityMessageService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly EmailBuildingStrategy _emailBuildingStrategy;
        private static MailSettingsSectionGroup _mailSettings;

        public MailingService(IApplicationSettings appSettings)
        {
            _telemetryClient = new TelemetryClient();
            System.Configuration.Configuration config = WebConfigurationManager.OpenWebConfiguration(HttpRuntime.AppDomainAppVirtualPath);
            _mailSettings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");
            _emailBuildingStrategy = appSettings.EmailBuildingStrategy;
        }

        public static bool HasSmtpServerConfigured(string appPath)
        {
            if (_mailSettings?.Smtp == null)
            {
                return false;
            }

            if (_mailSettings.Smtp.SpecifiedPickupDirectory != null && string.IsNullOrEmpty(_mailSettings.Smtp.SpecifiedPickupDirectory.PickupDirectoryLocation) == false)
            {
                return true;
            }

            if (_mailSettings.Smtp.Network != null && string.IsNullOrEmpty(_mailSettings.Smtp.Network.Host) == false)
            {
                return true;
            }

            return false;
        }

        public async Task SendAsync(IdentityMessage message)
        {
            await SendEmailInternalAsync(new EmailDto(message.Destination, message.Subject, message.Body));
        }

        public async Task SendEmailAsync(EmailDto email, bool skipDomainChange = false)
        {
            await SendEmailInternalAsync(email, skipDomainChange);
        }

        public async Task SendEmailsAsync(IEnumerable<EmailDto> emails, bool skipDomainChange = false)
        {
            foreach (var email in emails)
            {
                await SendEmailAsync(email, skipDomainChange);
            }
        }

        private async Task SendEmailInternalAsync(EmailDto email, bool skipDomainChange = false)
        {
            if (!HasSmtpServerConfigured(HttpRuntime.AppDomainAppVirtualPath))
            {
                return;
            }

            if (!email.Receivers.Any())
            {
                return;
            }

            using var client = new SmtpClient();
            try
            {
                IEnumerable<MailMessage> messages = BuildMessages(email, skipDomainChange);
                foreach (MailMessage mailMessage in messages)
                {
                    await client.SendMailAsync(mailMessage);
                }
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

            if (email.Attachment != null)
            {
                mailMessage.Attachments.Add(email.Attachment);
            }

            mailMessage.Subject = email.Subject;
            mailMessage.Body = email.Body;
            mailMessage.IsBodyHtml = true;

            return mailMessage;
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
