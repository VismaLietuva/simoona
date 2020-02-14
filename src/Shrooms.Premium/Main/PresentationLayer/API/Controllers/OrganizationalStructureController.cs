using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.OrganizationalStructure;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.OrganizationalStructure;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.OrganizationalStructure;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Api.Filters;

namespace Shrooms.Premium.Main.PresentationLayer.API.Controllers
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