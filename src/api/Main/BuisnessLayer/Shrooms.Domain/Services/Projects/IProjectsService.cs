using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Projects;

namespace Shrooms.Domain.Services.Projects
{
    public interface IProjectsService
    {
        Task<IEnumerable<ProjectsListItemDto>> GetProjects(UserAndOrganizationDTO userOrg);

        Task<IEnumerable<ProjectsAutoCompleteDto>> GetProjectsForAutocomplete(string name, int organizationId);

        Task NewProject(NewProjectDto dto);

        Task<ProjectDetailsDto> GetProjectDetails(int projectId, UserAndOrganizationDTO userAndOrganizationDTO);

        Task EditProject(EditProjectDto dto);

        Task Delete(int id, UserAndOrganizationDTO userOrg);

        Task<EditProjectDisplayDto> GetProjectById(int projectId, UserAndOrganizationDTO userOrg);

        Task ExpelMember(UserAndOrganizationDTO userAndOrganizationDTO, int projectId, string userId);

        void AddProjectsToUser(string userId, IEnumerable<int> projectIds, UserAndOrganizationDTO userOrg);

        bool ValidateManagerId(string userId, string managerId);
    }
}
