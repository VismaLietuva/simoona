using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Support;

namespace Shrooms.Domain.Services.Support
{
    public interface ISupportService
    {
        void SubmitTicket(UserAndOrganizationDTO userAndOrganization, SupportDto support);
    }
}