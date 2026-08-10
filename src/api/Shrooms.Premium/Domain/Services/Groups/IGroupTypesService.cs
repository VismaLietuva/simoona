using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Groups;

namespace Shrooms.Premium.Domain.Services.Groups
{
    public interface IGroupTypesService
    {
        /// <summary>
        /// Readable by everyone - creating a group needs to know which types exist and
        /// who may create them. Kudos configuration is redacted for non-kudos-admins.
        /// </summary>
        Task<IEnumerable<GroupTypeDto>> GetAllAsync(UserAndOrganizationDto userAndOrg);

        Task<GroupTypeDto> GetAsync(int organizationId, int id);

        Task CreateAsync(CreateGroupTypeDto dto);

        Task UpdateAsync(UpdateGroupTypeDto dto);

        Task DeleteAsync(int id, UserAndOrganizationDto userAndOrg);
    }
}
