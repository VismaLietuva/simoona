using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Groups;

namespace Shrooms.Premium.Domain.Services.Groups
{
    public interface IGroupKudosService
    {
        Task<IEnumerable<GroupKudosAllocationDto>> GetAllocationsAsync(int organizationId, int year, int month);

        Task<IList<GroupMonthlyKudosResultDto>> AwardOutstandingMonthsAsync(
            UserAndOrganizationDto userAndOrg,
            int year,
            int month);

        Task<GroupMonthlyKudosResultDto> AwardMonthlyKudosAsync(
            UserAndOrganizationDto userAndOrg,
            int year,
            int month);
    }
}
