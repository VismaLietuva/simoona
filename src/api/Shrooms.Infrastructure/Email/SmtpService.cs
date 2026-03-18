using System.Collections.Generic;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

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

            return !string.IsNullOrEmpty(host) || !string.IsNullOrEmpty(pickupDirectory);
        }

        /// <summary>
        /// Sends mail messages via SMTP asynchronously.
        /// </summary>
        /// <param name="messages">Message collection for sending.</param>
        /// <returns>A <see cref="Task"/> that represents asynchronous operation.</returns>
        public async Task SendAsync(IEnumerable<MimeMessage> messages)
        {
            var pickupDirectory = _configuration["Smtp:PickupDirectoryLocation"];
            if (!string.IsNullOrEmpty(pickupDirectory))
            {
                foreach (var message in messages)
                {
                    var filePath = System.IO.Path.Combine(pickupDirectory, $"{System.Guid.NewGuid()}.eml");
                    await message.WriteToAsync(filePath);
                }

                return;
            }

            var host = _configuration["Smtp:Host"];
            var port = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 587;
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var useSsl = bool.TryParse(_configuration["Smtp:UseSsl"], out var ssl) && ssl;

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            if (!string.IsNullOrEmpty(username))
            {
                await client.AuthenticateAsync(username, password);
            }

            foreach (var message in messages)
            {
                await client.SendAsync(message);
            }

            await client.DisconnectAsync(true);
        }
    }
}
