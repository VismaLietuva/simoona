using System.Linq;
using System.Net.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using Microsoft.AspNet.Identity;
using Shrooms.DataTransferObjects.Models.Emails;
using Shrooms.Infrastructure.Configuration;

namespace Shrooms.Infrastructure.Email
{
    public class MailingService : IMailingService, IIdentityMessageService
    {
        public MailingService()
        {
        }

        public static bool HasSmtpServerConfigured(string appPath)
        {
            var config = WebConfigurationManager.OpenWebConfiguration(appPath);
            var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");
            if (settings == null || settings.Smtp == null)
            {
                return false;
            }

            if (settings.Smtp.SpecifiedPickupDirectory != null && string.IsNullOrEmpty(settings.Smtp.SpecifiedPickupDirectory.PickupDirectoryLocation) == false)
            {
                return true;
            }

            if (settings.Smtp.Network != null && string.IsNullOrEmpty(settings.Smtp.Network.Host) == false)
            {
                return true;
            }

            return false;
        }

        public async Task SendAsync(IdentityMessage message)
        {
            if (!HasSmtpServerConfigured(HttpRuntime.AppDomainAppVirtualPath))
            {
                return;
            }

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.SendMailAsync(BuildMessage(new EmailDto(message.Destination, message.Subject, message.Body)));
                }
                catch (SmtpException)
                {
                }
            }

            return;
        }

        public void SendEmail(EmailDto email, bool skipDomainChange = false)
        {
            if (!HasSmtpServerConfigured(HttpRuntime.AppDomainAppVirtualPath))
            {
                return;
            }

            if (email.Receivers.Count() < 1)
            {
                return;
            }

            using (var client = new SmtpClient())
            {
                try
                {
                    client.Send(BuildMessage(email, skipDomainChange));
                }
                catch (SmtpException)
                {
                }
            }
        }

        private string ChangeEmailDomain(string senderEmail, string senderFullName)
          => $"{senderFullName} <{new MailAddress(senderEmail).User}@simoona.com>";

        private MailMessage BuildMessage(EmailDto email, bool skipDomainChange = false)
        {
            var mailMessage = new MailMessage();

            string sender = skipDomainChange
                ? $"{email.SenderFullName} <{email.SenderEmail}>"
                : ChangeEmailDomain(email.SenderEmail, email.SenderFullName);

            mailMessage.From = new MailAddress(sender);
            foreach (var receiver in email.Receivers)
            {
                mailMessage.To.Add(receiver);
            }

            mailMessage.Subject = email.Subject;
            mailMessage.Body = email.Body;
            mailMessage.IsBodyHtml = true;

            return mailMessage;
        }
    }
}
