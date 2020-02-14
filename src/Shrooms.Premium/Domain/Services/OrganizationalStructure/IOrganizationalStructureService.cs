using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.OrganizationalStructure;

namespace Shrooms.Premium.Domain.Services.OrganizationalStructure
{
    public interface IOrganizationalStructureService
    {
        OrganizationalStructureDTO GetOrganizationalStructure(UserAndOrganizationDTO userAndOrg);
    }
}
