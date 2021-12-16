using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.VacationPages;
using Shrooms.Domain.Services.VacationPages;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models.VacationPage;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [RoutePrefix("VacationPage")]
    public class VacationPageController : BaseController
    {
        private readonly IVacationPageService _vacationPageService;
        private readonly IMapper _mapper;

        public VacationPageController(IVacationPageService vacationPageService, IMapper mapper)
        {
            _vacationPageService = vacationPageService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("Get")]
        public async Task<IHttpActionResult> GetVacationPage()
        {
            var vacationPageDto = await _vacationPageService.GetVacationPage(GetOrganizationId());

            if (vacationPageDto == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<VacationPageDto, VacationPageViewModel>(vacationPageDto));
        }

        [HttpPut]
        [Route("Edit")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Vacation)]
        public async Task<IHttpActionResult> EditVacationPage(VacationPageViewModel vacationPageViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var userAndOrg = GetUserAndOrganization();
            var vacationPageDto = _mapper.Map<VacationPageViewModel, VacationPageDto>(vacationPageViewModel);

            await _vacationPageService.EditVacationPage(userAndOrg, vacationPageDto);

            return Ok();
        }
    }
}