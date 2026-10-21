using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.TimeRegistration;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public interface ITimeTableService
    {
        Task<TimeTableDto> GetTimeTableAsync(string month, string employeeId, UserAndOrganizationDto userOrg);

        Task SaveEntriesAsync(string month, string employeeId, IList<TimeEntryChangeDto> changes, UserAndOrganizationDto userOrg);

        Task SetAutoFillAsync(int projectId, string employeeId, UserAndOrganizationDto userOrg);

        Task AddProjectAsync(int projectId, string employeeId, UserAndOrganizationDto userOrg);

        Task RemoveProjectAsync(int projectId, string employeeId, UserAndOrganizationDto userOrg);
    }
}
