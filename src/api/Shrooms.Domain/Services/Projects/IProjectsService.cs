using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Projects;

namespace Shrooms.Domain.Services.Projects
{
    public interface IProjectsService
    {
        Task<IEnumerable<ProjectsListItemDto>> GetProjectsAsync(UserAndOrganizationDTO userOrg);

        Task<IEnumerable<ProjectsAutoCompleteDto>> GetProjectsForAutocompleteAsync(string name, int organizationId);

        Task NewProjectAsync(NewProjectDto dto);

        Task<ProjectDetailsDto> GetProjectDetailsAsync(int projectId, UserAndOrganizationDTO userAndOrganizationDTO);

        Task EditProjectAsync(EditProjectDto dto);

        Task DeleteAsync(int id, UserAndOrganizationDTO userOrg);

        Task<EditProjectDisplayDto> GetProjectByIdAsyncAsync(int projectId, UserAndOrganizationDTO userOrg);

        Task ExpelMemberAsync(UserAndOrganizationDTO userAndOrganizationDTO, int projectId, string userId);

        Task AddProjectsToUserAsync(string userId, IEnumerable<int> projectIds, UserAndOrganizationDTO userOrg);

        Task<bool> ValidateManagerIdAsync(string userId, string managerId);
    }
}
