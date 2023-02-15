using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Contracts.Infrastructure.Email
{
    public interface IMailingService
    {
        Task SendEmailAsync(EmailDto email, bool skipDomainChange = false);

        Task SendEmailsAsync(IEnumerable<EmailDto> emails, bool skipDomainChange = false);
    }
}
