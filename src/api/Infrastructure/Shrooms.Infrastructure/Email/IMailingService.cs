using Shrooms.DataTransferObjects.Models.Emails;

namespace Shrooms.Infrastructure.Email
{
    public interface IMailingService
    {
        void SendEmail(EmailDto email, bool skipDomainChange = false);
    }
}
