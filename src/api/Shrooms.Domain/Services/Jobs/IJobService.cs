using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Jobs;

namespace Shrooms.Domain.Services.Jobs
{
    public interface IJobService
    {
        Task CreateJobTypeAsync(JobTypeDto dto);

        Task UpdateJobTypeAsync(JobTypeDto dto);

        Task RemoveJobTypeAsync(int id, UserAndOrganizationDto userOrg);

        Task<JobTypeDto> GetJobTypeAsync(int id, UserAndOrganizationDto userOrg);

        Task<IEnumerable<JobTypeDto>> GetJobTypesAsync(UserAndOrganizationDto userAndOrg);
    }
}
