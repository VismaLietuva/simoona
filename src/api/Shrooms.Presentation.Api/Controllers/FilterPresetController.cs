using System;
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

            try
            {
                await _filterPresetService.UpdateAsync(editDto, GetUserAndOrganization());
                
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            throw new NotImplementedException();
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