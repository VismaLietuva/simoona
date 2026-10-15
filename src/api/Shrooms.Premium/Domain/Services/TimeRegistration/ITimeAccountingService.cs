using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.TimeRegistration;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public interface ITimeAccountingService
    {
        Task<IEnumerable<TimeCustomerDto>> GetCustomersAsync(UserAndOrganizationDto userOrg);

        Task<int> SaveCustomerAsync(TimeCustomerInputDto input, UserAndOrganizationDto userOrg);

        Task DeleteCustomerAsync(int id, UserAndOrganizationDto userOrg);

        Task<IEnumerable<TimeProjectDto>> GetProjectsAsync(UserAndOrganizationDto userOrg);

        Task<int> SaveProjectAsync(TimeProjectInputDto input, UserAndOrganizationDto userOrg);

        Task SetProjectArchivedAsync(int id, bool isArchived, UserAndOrganizationDto userOrg);

        Task DeleteProjectAsync(int id, UserAndOrganizationDto userOrg);

        Task<int> SaveTaskAsync(TimeProjectTaskInputDto input, UserAndOrganizationDto userOrg);

        Task DeleteTaskAsync(int id, UserAndOrganizationDto userOrg);
    }
}
