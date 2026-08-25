using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.VideoLibrary;
using Shrooms.Presentation.Api.Filters;
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
    [Route("VideoLibrary")]
    public class VideoLibraryController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IVideoLibraryService _videoLibraryService;

        public VideoLibraryController(IMapper mapper, IVideoLibraryService videoLibraryService)
        {
            _mapper = mapper;
            _videoLibraryService = videoLibraryService;
        }

        [HttpGet]
        [Route("List")]
        [PermissionAnyOfAuthorize(BasicPermissions.VideoLibrary, AdministrationPermissions.VideoLibrary)]
        [ProducesResponseType(typeof(IEnumerable<VideoLibraryItemViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List()
        {
            var videoDtos = await _videoLibraryService.GetVideosAsync(GetUserAndOrganization());
            var viewModels = _mapper.Map<IEnumerable<VideoLibraryItemDto>, IEnumerable<VideoLibraryItemViewModel>>(videoDtos);

            return Ok(viewModels);
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = AdministrationPermissions.VideoLibrary)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create(NewVideoLibraryItemViewModel videoViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var videoDto = _mapper.Map<NewVideoLibraryItemViewModel, VideoLibraryItemDto>(videoViewModel);
            SetOrganizationAndUser(videoDto);

            try
            {
                await _videoLibraryService.CreateVideoAsync(videoDto);
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
        public async Task<IActionResult> Update(VideoLibraryItemViewModel videoViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var videoDto = _mapper.Map<VideoLibraryItemViewModel, VideoLibraryItemDto>(videoViewModel);
            SetOrganizationAndUser(videoDto);

            try
            {
                await _videoLibraryService.UpdateVideoAsync(videoDto);
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
                await _videoLibraryService.RemoveVideoAsync(id, GetUserAndOrganization());
                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}
