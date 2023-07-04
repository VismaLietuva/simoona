using System.Collections.Generic;
using System.Net.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace Shrooms.Infrastructure.Email
{
    /// <summary>
    /// Service that wraps SMTP client and SMTP mail settings.
    /// </summary>
    public class SmtpService : IMailSendingService
    {
        private static MailSettingsSectionGroup _mailSettings;

        public SmtpService()
        {
            _mailSettings = (MailSettingsSectionGroup)WebConfigurationManager
                .OpenWebConfiguration(HttpRuntime.AppDomainAppVirtualPath)
                .GetSectionGroup("system.net/mailSettings");
        }

        /// <summary>
        /// Determines if SMTP configuration is present and valid.
        /// </summary>
        public bool IsMailSenderConfigured()
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

        /// <summary>
        /// Sends mail messages via SMTP asynchronously.
        /// </summary>
        /// <param name="messages">Message collection for sending.</param>
        /// <returns>A <see cref="Task"/> that represents asynchronous operation.</returns>
        public async Task SendAsync(IEnumerable<MailMessage> messages)
        {
            using var client = new SmtpClient();
            foreach (MailMessage message in messages)
            {
                await client.SendMailAsync(message);
            }
        }
    }
}
