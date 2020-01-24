using Shrooms.DataTransferObjects.Models.Emails;

namespace Shrooms.Host.Contracts.Infrastructure.Email
{
    public interface IMailingService
    {
        void SendEmail(EmailDto email, bool skipDomainChange = false);
    }
}
