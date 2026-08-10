using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Groups;

namespace Shrooms.Premium.Domain.Services.Groups
{
    public interface IGroupsService
    {
        /// <summary>
        /// References that are not publicly visible are only returned for groups the
        /// caller belongs to, so the caller is needed rather than just the organization.
        /// </summary>
        Task<IEnumerable<GroupDto>> GetAllAsync(UserAndOrganizationDto userAndOrg);

        Task<GroupDto> GetAsync(UserAndOrganizationDto userAndOrg, int id);

        Task CreateAsync(GroupPostDto dto);

        Task UpdateAsync(GroupPostDto dto);

        Task DeleteAsync(int id, UserAndOrganizationDto userAndOrg);

        Task ApproveAsync(int id, UserAndOrganizationDto userAndOrg);
    }
}
