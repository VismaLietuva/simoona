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
    public class SmtpService : ISmtpService
    {
        private static MailSettingsSectionGroup _mailSettings;

        public SmtpService()
        {
            _mailSettings = (MailSettingsSectionGroup)WebConfigurationManager
                .OpenWebConfiguration(HttpRuntime.AppDomainAppVirtualPath)
                .GetSectionGroup("system.net/mailSettings");
        }

        /// <inheritdoc />
        public bool HasSmtpServerConfigured()
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

        /// <inheritdoc />
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
