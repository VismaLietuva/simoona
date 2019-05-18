using Shrooms.DataTransferObjects.Models;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.OrganizationalStructure;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.OrganizationalStructure
{
    public interface IOrganizationalStructureService
    {
        OrganizationalStructureDTO GetOrganizationalStructure(UserAndOrganizationDTO userAndOrg);
    }
}
