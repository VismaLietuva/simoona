using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.FilterPresets;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models.FilterPresets;

namespace Shrooms.Presentation.Api.Controllers
{
    [RoutePrefix("FilterPreset")]
    [Authorize]
    public class FilterPresetController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IFilterPresetService _filterPresetService;

        public FilterPresetController(IMapper mapper, IFilterPresetService filterPresetService)
        {
            _mapper = mapper;
            _filterPresetService = filterPresetService;
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Administration)]
        public async Task<IHttpActionResult> Create(CreateFilterPresetViewModel createViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createDto = _mapper.Map<CreateFilterPresetViewModel, CreateFilterPresetDto>(createViewModel);

            try
            {
                await _filterPresetService.CreateAsync(createDto, GetUserAndOrganization());
             
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}