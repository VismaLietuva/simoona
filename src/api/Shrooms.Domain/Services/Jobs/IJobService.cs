using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.Jobs;

namespace Shrooms.Domain.Services.Jobs
{
    public interface IJobService
    {
        Task CreateJobType(JobTypeDTO dto);

        Task UpdateJobType(JobTypeDTO dto);

        Task RemoveJobType(int id, UserAndOrganizationDTO userOrg);

        Task<JobTypeDTO> GetJobType(int id, UserAndOrganizationDTO userOrg);

        Task<IEnumerable<JobTypeDTO>> GetJobTypes(UserAndOrganizationDTO userAndOrg);
    }
}
