using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.TimeRegistration;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public interface ITimePeriodService
    {
        Task SubmitAsync(string month, UserAndOrganizationDto userOrg);

        Task WithdrawAsync(string month, UserAndOrganizationDto userOrg);

        Task ApproveAsync(string employeeId, string month, UserAndOrganizationDto userOrg);

        Task UnapproveAsync(string employeeId, string month, UserAndOrganizationDto userOrg);

        Task LockAsync(string month, UserAndOrganizationDto userOrg);

        Task UnlockAsync(string month, UserAndOrganizationDto userOrg);

        Task<IEnumerable<TimeTeamMemberDto>> GetTeamAsync(string month, UserAndOrganizationDto userOrg);

        Task<IEnumerable<TimeTeamMemberDto>> GetEmployeesAsync(string month, UserAndOrganizationDto userOrg);

        Task<TimeManagerDto> GetManagerAsync(string employeeId, UserAndOrganizationDto userOrg);

        Task<TimeRegistrationOverviewDto> GetOverviewAsync(UserAndOrganizationDto userOrg);
    }
}
