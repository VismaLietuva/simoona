using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.TimeRegistration;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public interface ITimeOvertimeService
    {
        Task<IEnumerable<TimeOvertimeRequestDto>> GetForEmployeeAsync(string employeeId, UserAndOrganizationDto userOrg);

        Task<IEnumerable<TimeOvertimeRequestDto>> GetTeamAsync(UserAndOrganizationDto userOrg);

        Task<TimeOvertimeRequestDto> GetForReviewAsync(int id, UserAndOrganizationDto userOrg);

        Task<int> CreateAsync(TimeOvertimeRequestInputDto input, UserAndOrganizationDto userOrg);

        Task ApproveAsync(int id, UserAndOrganizationDto userOrg);

        Task RejectAsync(int id, string comment, UserAndOrganizationDto userOrg);

        Task CancelAsync(int id, UserAndOrganizationDto userOrg);
    }
}
