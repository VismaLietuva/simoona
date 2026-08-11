using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Groups;

namespace Shrooms.Premium.Domain.Services.Groups
{
    public interface IGroupKudosService
    {
        /// <summary>
        /// One allocation per user: the highest single monthly amount across the
        /// kudos-receiving groups they belonged to during the given month.
        /// </summary>
        Task<IEnumerable<GroupKudosAllocationDto>> GetAllocationsAsync(int organizationId, int year, int month);

        Task<GroupMonthlyKudosResultDto> AwardMonthlyKudosAsync(
            UserAndOrganizationDto userAndOrg,
            int year,
            int month);
    }
}
