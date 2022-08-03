using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks;

namespace Shrooms.Domain.Services.ExternalLinks
{
    public interface IExternalLinkService
    {
        Task<IEnumerable<ExternalLinkDto>> GetAllAsync(int organizationId);

        Task<ExternalLinkDto> FindAsync(int externalLinkId, UserAndOrganizationDto userOrg);

        Task UpdateLinksAsync(ManageExternalLinkDto manageLinksDto);
    }
}
