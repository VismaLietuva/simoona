using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Shrooms.Infrastructure.Email
{
    /// <summary>
    /// Service that wraps SMTP client and SMTP mail settings.
    /// </summary>
    public interface ISmtpService
    {
        /// <summary>
        /// Sends mail messages via SMTP asynchronously.
        /// </summary>
        /// <param name="messages">Message collection for sending.</param>
        /// <returns>A <see cref="Task"/> that represents asynchronous operation.</returns>
        Task SendAsync(IEnumerable<MailMessage> messages);

        /// <summary>
        /// Determines if SMTP configuration is present and valid.
        /// </summary>
        bool HasSmtpServerConfigured();
    }
}
