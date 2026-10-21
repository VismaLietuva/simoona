using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts.ScheduledPosts;
using Shrooms.Presentation.Api.BackgroundWorkers;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.Common.Controllers;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [Route("ScheduledPost")]
    public class ScheduledPostController : BaseController
    {
        private readonly IScheduledPostService _scheduledPostService;
        private readonly IWallService _wallService;
        private readonly IPermissionService _permissionService;
        private readonly IAsyncRunner _asyncRunner;

        public ScheduledPostController(IScheduledPostService scheduledPostService, IWallService wallService, IPermissionService permissionService, IAsyncRunner asyncRunner)
        {
            _scheduledPostService = scheduledPostService;
            _wallService = wallService;
            _permissionService = permissionService;
            _asyncRunner = asyncRunner;
        }

        [HttpGet]
        [Route("List")]
        [PermissionAnyOfAuthorize(BasicPermissions.Post, BasicPermissions.Event)]
        [ProducesResponseType(typeof(IEnumerable<ScheduledPostDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List(int? wallId)
        {
            return Ok(await _scheduledPostService.GetScheduledPostsAsync(wallId, GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAnyOfAuthorize(BasicPermissions.Post, BasicPermissions.Event)]
        [ProducesResponseType(typeof(ScheduledPostDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] NewScheduledPostDto dto)
        {
            try
            {
                if (!await CanPostOnWallAsync(dto.WallId, GetUserAndOrganization()))
                {
                    return Forbidden();
                }

                SetOrganizationAndUser(dto);

                return Ok(await _scheduledPostService.CreateAsync(dto));
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [Route("Edit")]
        [PermissionAnyOfAuthorize(BasicPermissions.Post, BasicPermissions.Event)]
        [ProducesResponseType(typeof(ScheduledPostDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Edit([FromBody] EditScheduledPostDto dto)
        {
            SetOrganizationAndUser(dto);

            try
            {
                return Ok(await _scheduledPostService.EditAsync(dto));
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        [PermissionAnyOfAuthorize(BasicPermissions.Post, BasicPermissions.Event)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _scheduledPostService.DeleteAsync(id, GetUserAndOrganization());

                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPost]
        [Route("SendNow")]
        [PermissionAnyOfAuthorize(BasicPermissions.Post, BasicPermissions.Event)]
        public async Task<IActionResult> SendNow(int id)
        {
            try
            {
                var createdPost = await _scheduledPostService.PublishNowAsync(id, GetUserAndOrganization());

                if (createdPost != null)
                {
                    var userHubDto = GetUserAndOrganizationHub();
                    _asyncRunner.Run<PostNotifier>(async notifier => await notifier.NotifyAboutNewPostAsync(createdPost, userHubDto), GetOrganizationName());
                }

                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        private async Task<bool> CanPostOnWallAsync(int wallId, UserAndOrganizationDto userAndOrg)
        {
            if (await _permissionService.UserHasPermissionAsync(userAndOrg, BasicPermissions.Post))
            {
                return true;
            }

            var wall = await _wallService.GetWallAsync(wallId, userAndOrg)
                ?? throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Wall not found");

            return wall.Type == WallType.Events || wall.Type == WallType.Polls;
        }
    }
}
