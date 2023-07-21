using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Premium.DataTransferObjects.Models.OrganizationalStructure;
using Shrooms.Premium.Domain.Services.OrganizationalStructure;
using Shrooms.Premium.Presentation.WebViewModels.OrganizationalStructure;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;

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
        public async Task<IHttpActionResult> GetOrganizationalStructure()
        {
            var organizationDto = await _organizationalStructureService.GetOrganizationalStructureAsync(GetUserAndOrganization());
            var resultViewModel = _mapper.Map<OrganizationalStructureDto, OrganizationalStructureViewModel>(organizationDto);
            return Ok(resultViewModel);
        }
    }
}
