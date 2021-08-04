using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Projects;

namespace Shrooms.Domain.Services.Projects
{
    public interface IProjectsService
    {
        Task<IEnumerable<ProjectsListItemDto>> GetProjectsAsync(UserAndOrganizationDto userOrg);

        Task<IEnumerable<ProjectsAutoCompleteDto>> GetProjectsForAutocompleteAsync(string name, int organizationId);

        Task NewProjectAsync(NewProjectDto dto);

        Task<ProjectDetailsDto> GetProjectDetailsAsync(int projectId, UserAndOrganizationDto userAndOrganizationDto);

        Task EditProjectAsync(EditProjectDto dto);

        Task DeleteAsync(int id, UserAndOrganizationDto userOrg);

        Task<EditProjectDisplayDto> GetProjectByIdAsyncAsync(int projectId, UserAndOrganizationDto userOrg);

        Task ExpelMemberAsync(UserAndOrganizationDto userAndOrganizationDto, int projectId, string userId);

        Task AddProjectsToUserAsync(string userId, IEnumerable<int> projectIds, UserAndOrganizationDto userOrg);

        Task<bool> ValidateManagerIdAsync(string userId, string managerId);
    }
}
