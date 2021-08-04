using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Support;

namespace Shrooms.Domain.Services.Support
{
    public interface ISupportService
    {
        Task SubmitTicketAsync(UserAndOrganizationDto userAndOrganization, SupportDto support);
    }
}