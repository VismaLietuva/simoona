using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.VideoLibrary;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.WebViewModels.Models.VideoLibrary;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [Route("VideoType")]
    public class VideoTypeController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IVideoTypeService _videoTypeService;

        public VideoTypeController(IMapper mapper, IVideoTypeService videoTypeService)
        {
            _mapper = mapper;
            _videoTypeService = videoTypeService;
        }

        [HttpGet]
        [Route("Types")]
        [PermissionAuthorize(Permission = AdministrationPermissions.VideoLibrary)]
        [ProducesResponseType(typeof(IEnumerable<VideoTypeViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVideoTypes()
        {
            var videoTypeDtos = await _videoTypeService.GetVideoTypesAsync(GetUserAndOrganization());
            var viewModels = _mapper.Map<IEnumerable<VideoTypeDto>, IEnumerable<VideoTypeViewModel>>(videoTypeDtos);

            return Ok(viewModels);
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = AdministrationPermissions.VideoLibrary)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create(NewVideoTypeViewModel videoTypeViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var videoTypeDto = _mapper.Map<NewVideoTypeViewModel, VideoTypeDto>(videoTypeViewModel);
            SetOrganizationAndUser(videoTypeDto);

            try
            {
                await _videoTypeService.CreateVideoTypeAsync(videoTypeDto);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [Route("Update")]
        [PermissionAuthorize(Permission = AdministrationPermissions.VideoLibrary)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(VideoTypeViewModel videoTypeViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var videoTypeDto = _mapper.Map<VideoTypeViewModel, VideoTypeDto>(videoTypeViewModel);
            SetOrganizationAndUser(videoTypeDto);

            try
            {
                await _videoTypeService.UpdateVideoTypeAsync(videoTypeDto);
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        [PermissionAuthorize(Permission = AdministrationPermissions.VideoLibrary)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id < 1)
            {
                return BadRequest();
            }

            try
            {
                await _videoTypeService.RemoveVideoTypeAsync(id, GetUserAndOrganization());
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}
