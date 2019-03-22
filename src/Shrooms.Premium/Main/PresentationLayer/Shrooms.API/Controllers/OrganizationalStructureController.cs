using AutoMapper;
using Shrooms.API.Controllers.WebApi;
using Shrooms.API.Filters;
using Shrooms.DataTransferObjects.Models.OrganizationalStructure;
using Shrooms.WebViewModels.Models.OrganizationalStructure;
using System.Web.Http;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Domain.Services.OrganizationalStructure;

namespace Shrooms.API.Controllers
{
    [Authorize]
    public class OrganizationalStructureController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IOrganizationalStructureService _organizationalStructureService;

        public OrganizationalStructureController(IMapper mapper, IOrganizationalStructureService organizationalStructureService)
        {
            _mapper = mapper;
            _organizationalStructureService = organizationalStructureService;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.OrganizationalStructure)]
        public IHttpActionResult GetOrganizationalStructure()
        {
            var resultDTO = _organizationalStructureService.GetOrganizationalStructure(GetUserAndOrganization());
            var resultViewModel = _mapper.Map<OrganizationalStructureDTO, OrganizationalStructureViewModel>(resultDTO);
            return Ok(resultViewModel);
        }
    }
}