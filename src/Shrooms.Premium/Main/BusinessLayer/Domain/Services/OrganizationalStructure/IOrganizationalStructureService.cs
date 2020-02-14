using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.OrganizationalStructure;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.OrganizationalStructure
{
    public interface IOrganizationalStructureService
    {
        OrganizationalStructureDTO GetOrganizationalStructure(UserAndOrganizationDTO userAndOrg);
    }
}
