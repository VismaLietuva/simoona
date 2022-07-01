using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Enums;
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
        [Route("Update")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<IHttpActionResult> Update(AddEditDeleteFilterPresetViewModel updateViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updateDto = _mapper.Map<AddEditDeleteFilterPresetViewModel, AddEditDeleteFilterPresetDto>(updateViewModel);
                
                updateDto.UserOrg = GetUserAndOrganization();

                var updatedDto = await _filterPresetService.UpdateAsync(updateDto);
                var updatedViewModel = _mapper.Map<UpdatedFilterPresetDto, UpdatedFilterPresetViewModel>(updatedDto);

                return Ok(updatedViewModel);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [Route("GetPresetsForPage")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<IHttpActionResult> GetPresets(PageType pageType) 
        {
            try
            {
                var presets = await _filterPresetService.GetPresetsForPageAsync(pageType, GetOrganizationId());
                var presetsViewModel = _mapper.Map<IEnumerable<FilterPresetDto>, IEnumerable<FilterPresetViewModel>>(presets);

                return Ok(presetsViewModel);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }


        [HttpGet]
        [Route("GetFilters")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Administration)]
        public async Task<IHttpActionResult> GetFilters([FromUri] FilterType[] filterTypes)
        {
            try
            {
                var filtersDto = await _filterPresetService.GetFiltersAsync(filterTypes, GetOrganizationId());
                var filtersViewModel = _mapper.Map<IEnumerable<FiltersDto>, IEnumerable<FiltersViewModel>>(filtersDto);

                return Ok(filtersViewModel);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}