using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Shrooms.Infrastructure.Email
{
    /// <summary>
    /// Service that wraps mail sending client.
    /// </summary>
    public interface IMailSendingService
    {
        /// <summary>
        /// Sends mail messages asynchronously.
        /// </summary>
        /// <param name="messages">Message collection for sending.</param>
        /// <returns>A <see cref="Task"/> that represents asynchronous operation.</returns>
        Task SendAsync(IEnumerable<MailMessage> messages);

        /// <summary>
        /// Determines if mail sender service is configured and ready.
        /// </summary>
        bool IsMailSenderConfigured();
    }
}
