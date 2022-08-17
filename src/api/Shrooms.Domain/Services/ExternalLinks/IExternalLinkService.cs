using Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Domain.Services.ExternalLinks
{
    public interface IExternalLinkService
    {
        Task<IEnumerable<ExternalLinkDto>> GetAllAsync(int organizationId);

        Task<ExternalLinkDto> GetAsync(int externalLinkId, UserAndOrganizationDto userOrg);

        Task UpdateLinksAsync(ManageExternalLinkDto manageLinksDto);
    }
}
