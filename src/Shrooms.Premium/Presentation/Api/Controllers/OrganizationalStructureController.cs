using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Premium.DataTransferObjects.Models.OrganizationalStructure;
using Shrooms.Premium.Domain.Services.OrganizationalStructure;
using Shrooms.Premium.Presentation.WebViewModels.OrganizationalStructure;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Api.Filters;

namespace Shrooms.Premium.Presentation.Api.Controllers
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