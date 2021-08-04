using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Support;

namespace Shrooms.Domain.Services.Support
{
    public interface ISupportService
    {
        Task SubmitTicketAsync(UserAndOrganizationDTO userAndOrganization, SupportDto support);
    }
}