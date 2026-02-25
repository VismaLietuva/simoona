using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Shrooms.Infrastructure.Email
{
    /// <summary>
    /// Service that wraps SMTP client and SMTP mail settings.
    /// </summary>
    public class SmtpService : IMailSendingService
    {
        private readonly IConfiguration _configuration;

        public SmtpService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Determines if SMTP configuration is present and valid.
        /// </summary>
        public bool IsMailSenderConfigured()
        {
            var host = _configuration["Smtp:Host"];
            var pickupDirectory = _configuration["Smtp:PickupDirectoryLocation"];

            if (!string.IsNullOrEmpty(pickupDirectory))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(host))
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
