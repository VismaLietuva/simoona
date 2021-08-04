using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.OrganizationalStructure;

namespace Shrooms.Premium.Domain.Services.OrganizationalStructure
{
    public interface IOrganizationalStructureService
    {
        Task<OrganizationalStructureDTO> GetOrganizationalStructureAsync(UserAndOrganizationDTO userAndOrg);
    }
}
