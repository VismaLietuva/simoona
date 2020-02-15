using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Support;

namespace Shrooms.Domain.Services.Support
{
    public interface ISupportService
    {
        void SubmitTicket(UserAndOrganizationDTO userAndOrganization, SupportDto support);
    }
}