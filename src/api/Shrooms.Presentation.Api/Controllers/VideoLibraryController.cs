using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.VideoLibrary;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.ViewModels;
using Shrooms.Domain.Extensions;
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
        [ProducesResponseType(typeof(PagedViewModel<VideoLibraryItemViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] VideoLibraryListArgsViewModel argsViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            argsViewModel ??= new VideoLibraryListArgsViewModel();

            var argsDto = _mapper.Map<VideoLibraryListArgsViewModel, VideoLibraryListArgsDto>(argsViewModel);
            SetOrganizationAndUser(argsDto);

            var pagedVideoDtos = await _videoLibraryService.GetVideosAsync(argsDto);
            var viewModels = _mapper.Map<IEnumerable<VideoLibraryItemDto>, IEnumerable<VideoLibraryItemViewModel>>(pagedVideoDtos);

            return Ok(pagedVideoDtos.ToPagedViewModel(viewModels, argsViewModel));
        }

        [HttpGet]
        [Route("Filters")]
        [PermissionAnyOfAuthorize(BasicPermissions.VideoLibrary, AdministrationPermissions.VideoLibrary)]
        [ProducesResponseType(typeof(VideoLibraryFiltersViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Filters()
        {
            var filtersDto = await _videoLibraryService.GetFiltersAsync(GetUserAndOrganization());
            var viewModel = _mapper.Map<VideoLibraryFiltersDto, VideoLibraryFiltersViewModel>(filtersDto);

            return Ok(viewModel);
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAnyOfAuthorize(BasicPermissions.VideoLibrary, AdministrationPermissions.VideoLibrary)]
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
