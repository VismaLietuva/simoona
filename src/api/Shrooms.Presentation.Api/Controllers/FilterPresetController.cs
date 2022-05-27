using System;
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

        [HttpGet]
        [Route("GetPresetsForPage")]
        [PermissionAuthorize(Permission = BasicPermissions.ApplicationUser)]
        public async Task<IHttpActionResult> GetPresets(PageType forPage) 
        {
            try
            {
                var presets = await _filterPresetService.GetPresetsForPageAsync(forPage, GetOrganizationId());
                var presetsViewModel = _mapper.Map<IEnumerable<FilterPresetDto>, IEnumerable<FilterPresetViewModel>>(presets);

                return Ok(presetsViewModel);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }


        [HttpPut]
        [Route("Edit")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Administration)]
        public async Task<IHttpActionResult> Edit(EditFilterPresetViewModel editViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var editDto = _mapper.Map<EditFilterPresetViewModel, EditFilterPresetDto>(editViewModel);
            var userOrg = GetUserAndOrganization();

            editDto.UserId = userOrg.UserId;
            editDto.OrganizationId = userOrg.OrganizationId;

            try
            {
                await _filterPresetService.UpdateAsync(editDto);
                
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Administration)]
        public async Task<IHttpActionResult> Create(CreateFilterPresetViewModel createViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var createDto = _mapper.Map<CreateFilterPresetViewModel, CreateFilterPresetDto>(createViewModel);
            var userOrg = GetUserAndOrganization();

            createDto.UserId = userOrg.UserId;
            createDto.OrganizationId = userOrg.OrganizationId;

            try
            {
                await _filterPresetService.CreateAsync(createDto);
             
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}