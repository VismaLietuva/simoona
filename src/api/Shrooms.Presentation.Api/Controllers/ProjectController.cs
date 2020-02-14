using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Projects;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Domain.Services.Projects;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models.Projects;

namespace Shrooms.Presentation.Api.Controllers
{
    [RoutePrefix("Project")]
    [Authorize]
    public class ProjectController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IProjectsService _projectsService;

        public ProjectController(IMapper mapper, IProjectsService projectsService)
        {
            _mapper = mapper;
            _projectsService = projectsService;
        }

        [HttpGet]
        [Route("List")]
        [PermissionAuthorize(Permission = BasicPermissions.Project)]
        public async Task<IHttpActionResult> GetProjects()
        {
            var projectsDto = await _projectsService.GetProjects(GetUserAndOrganization());
            var result = _mapper.Map<IEnumerable<ProjectsListItemDto>, IEnumerable<ProjectsListItemViewModel>>(projectsDto);

            return Ok(result);
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Project)]
        public async Task<IHttpActionResult> NewProject(NewProjectViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dto = _mapper.Map<NewProjectViewModel, NewProjectDto>(viewModel);
            SetOrganizationAndUser(dto);

            try
            {
                await _projectsService.NewProject(dto);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }

        [HttpGet]
        [Route("Edit")]
        [PermissionAuthorize(Permission = BasicPermissions.Project)]
        public async Task<IHttpActionResult> GetProjectForEdit(int id)
        {
            if (id < 1)
            {
                return BadRequest();
            }

            try
            {
                var resultDto = await _projectsService.GetProjectById(id, GetUserAndOrganization());
                var viewModel = _mapper.Map<EditProjectDisplayDto, EditProjectDisplayViewModel>(resultDto);
                return Ok(viewModel);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
            catch (UnauthorizedException)
            {
                return Forbidden();
            }
        }

        [HttpPut]
        [Route("Edit")]
        [PermissionAuthorize(Permission = BasicPermissions.Project)]
        public async Task<IHttpActionResult> Update(EditProjectViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dto = _mapper.Map<EditProjectViewModel, EditProjectDto>(viewModel);
            SetOrganizationAndUser(dto);

            try
            {
                await _projectsService.EditProject(dto);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
            catch (UnauthorizedException)
            {
                return Forbidden();
            }
        }

        [HttpGet]
        [Route("AutoComplete")]
        [PermissionAuthorize(Permission = BasicPermissions.Project)]
        public async Task<IHttpActionResult> GetForAutoComplete(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("search string cannot be empty");
            }

            var projectsDTO = await _projectsService.GetProjectsForAutocomplete(name, GetUserAndOrganization().OrganizationId);
            var result = _mapper.Map<IEnumerable<ProjectsAutoCompleteDto>, IEnumerable<ProjectsBasicInfoViewModel>>(projectsDTO);

            return Ok(result);
        }

        [HttpDelete]
        [Route("Delete")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Project)]
        public async Task<IHttpActionResult> Delete(int id)
        {
            if (id < 1)
            {
                return BadRequest();
            }

            try
            {
                await _projectsService.Delete(id, GetUserAndOrganization());
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
            catch (UnauthorizedException)
            {
                return Forbidden();
            }
        }

        [HttpGet]
        [Route("Details")]
        [PermissionAuthorize(Permission = BasicPermissions.Project)]
        public async Task<IHttpActionResult> GetProjectDetails(int projectId)
        {
            try
            {
                var projectDto = await _projectsService.GetProjectDetails(projectId, GetUserAndOrganization());
                var result = _mapper.Map<ProjectDetailsDto, ProjectDetailsViewModel>(projectDto);
                return Ok(result);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpDelete]
        [Route("ExpelMember")]
        [PermissionAuthorize(Permission = BasicPermissions.Project)]
        public async Task<IHttpActionResult> ExpelMember(int projectId, string userId)
        {
            try
            {
                await _projectsService.ExpelMember(GetUserAndOrganization(), projectId, userId);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
            catch (UnauthorizedException)
            {
                return Forbidden();
            }
        }
    }
}