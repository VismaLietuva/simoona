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
            var host = _configuration["SmtpHost"];
            var pickupDirectory = _configuration["SmtpPickupDirectoryLocation"];

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
            var pickupDirectory = _configuration["SmtpPickupDirectoryLocation"];
            if (!string.IsNullOrEmpty(pickupDirectory))
            {
                using var pickupClient = new SmtpClient
                {
                    DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                    PickupDirectoryLocation = pickupDirectory
                };
                foreach (var message in messages)
                {
                    await pickupClient.SendMailAsync(message);
                }
                return;
            }

            var host = _configuration["SmtpHost"];
            var port = int.TryParse(_configuration["SmtpPort"], out var p) ? p : 587;
            var username = _configuration["SmtpUserName"];
            var password = _configuration["SmtpPassword"];

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = port != 25,
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };

            if (!string.IsNullOrEmpty(username))
            {
                client.Credentials = new System.Net.NetworkCredential(username, password);
            }

            foreach (var message in messages)
            {
                await client.SendMailAsync(message);
            }
        }
    }
}
