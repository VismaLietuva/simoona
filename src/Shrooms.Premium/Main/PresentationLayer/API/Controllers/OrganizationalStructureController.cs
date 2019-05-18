using System.Web.Http;
using AutoMapper;
using Shrooms.API.Controllers;
using Shrooms.API.Filters;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.OrganizationalStructure;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.OrganizationalStructure;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.OrganizationalStructure;

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