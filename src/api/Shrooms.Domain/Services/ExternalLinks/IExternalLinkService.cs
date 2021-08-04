using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks;

namespace Shrooms.Domain.Services.ExternalLinks
{
    public interface IExternalLinkService
    {
        Task<IEnumerable<ExternalLinkDto>> GetAllAsync(int organizationId);
        Task UpdateLinksAsync(AddEditDeleteExternalLinkDto updateLinksDto);
    }
}
