using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Jobs;

namespace Shrooms.Domain.Services.Jobs
{
    public interface IJobService
    {
        Task CreateJobTypeAsync(JobTypeDTO dto);

        Task UpdateJobTypeAsync(JobTypeDTO dto);

        Task RemoveJobTypeAsync(int id, UserAndOrganizationDTO userOrg);

        Task<JobTypeDTO> GetJobTypeAsync(int id, UserAndOrganizationDTO userOrg);

        Task<IEnumerable<JobTypeDTO>> GetJobTypesAsync(UserAndOrganizationDTO userAndOrg);
    }
}
