using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Projects;

namespace Shrooms.Domain.Services.Projects
{
    public interface IProjectsService
    {
        Task<IEnumerable<ProjectsListItemDto>> GetProjects(UserAndOrganizationDTO userOrg);

        Task<IEnumerable<ProjectsAutoCompleteDto>> GetProjectsForAutocomplete(string name, int organizationId);

        Task NewProject(NewProjectDto dto);

        Task<ProjectDetailsDto> GetProjectDetails(int projectId, UserAndOrganizationDTO userAndOrganizationDTO);

        Task EditProjectAsync(EditProjectDto dto);

        Task DeleteAsync(int id, UserAndOrganizationDTO userOrg);

        Task<EditProjectDisplayDto> GetProjectByIdAsync(int projectId, UserAndOrganizationDTO userOrg);

        Task ExpelMemberAsync(UserAndOrganizationDTO userAndOrganizationDTO, int projectId, string userId);

        Task AddProjectsToUserAsync(string userId, IEnumerable<int> projectIds, UserAndOrganizationDTO userOrg);

        bool ValidateManagerId(string userId, string managerId);
    }
}
