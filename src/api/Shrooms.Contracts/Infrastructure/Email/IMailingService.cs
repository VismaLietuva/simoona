using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Contracts.Infrastructure.Email
{
    public interface IMailingService
    {
        void SendEmail(EmailDto email, bool skipDomainChange = false);
    }
}
