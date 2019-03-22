using System;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.OrganizationalStructure;

namespace Shrooms.Domain.Services.OrganizationalStructure
{
    public interface IOrganizationalStructureService
    {
        OrganizationalStructureDTO GetOrganizationalStructure(UserAndOrganizationDTO userAndOrg);
    }
}
