using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Polls;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Polls;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    public class PollController : BaseController
    {
        private readonly IPollService _pollService;
        private readonly IPermissionService _permissionService;

        public PollController(IPollService pollService, IPermissionService permissionService)
        {
            _pollService = pollService;
            _permissionService = permissionService;
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Poll)]
        [ProducesResponseType(typeof(IEnumerable<PollListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List()
        {
            return Ok(await _pollService.GetVisiblePollsAsync(GetUserAndOrganization()));
        }

        [HttpGet]
        [PermissionAuthorize(AdministrationPermissions.Poll)]
        [ProducesResponseType(typeof(IEnumerable<PollListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ManagedList()
        {
            return Ok(await _pollService.GetAllPollsAsync(GetUserAndOrganization()));
        }

        [HttpGet]
        [PermissionAuthorize(BasicPermissions.Poll)]
        [ProducesResponseType(typeof(PollDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            return await GuardedAsync(async () =>
            {
                var userOrg = GetUserAndOrganization();
                return await _pollService.GetPollAsync(id, userOrg, await CanManageAsync(userOrg));
            });
        }

        [HttpPost]
        [PermissionAuthorize(BasicPermissions.Poll)]
        [ProducesResponseType(typeof(PollDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] CreatePollDto dto)
        {
            return await GuardedAsync(async () =>
            {
                var userOrg = GetUserAndOrganization();
                dto.UserId = userOrg.UserId;
                dto.OrganizationId = userOrg.OrganizationId;

                return await _pollService.CreateAsync(dto, await CanManageAsync(userOrg));
            });
        }

        [HttpPut]
        [PermissionAuthorize(BasicPermissions.Poll)]
        public async Task<IActionResult> Update([FromBody] UpdatePollDto dto)
        {
            return await GuardedAsync(async () =>
            {
                var userOrg = GetUserAndOrganization();
                dto.UserId = userOrg.UserId;
                dto.OrganizationId = userOrg.OrganizationId;

                await _pollService.UpdateAsync(dto, await CanManageAsync(userOrg));
                return true;
            });
        }

        [HttpPost]
        [PermissionAuthorize(AdministrationPermissions.Poll)]
        public async Task<IActionResult> Publish([FromBody] PollReviewArgsDto args)
        {
            return await GuardedAsync(async () =>
            {
                await _pollService.PublishAsync(WithUser(args));
                return true;
            });
        }

        [HttpPost]
        [PermissionAuthorize(AdministrationPermissions.Poll)]
        public async Task<IActionResult> Reject([FromBody] PollReviewArgsDto args)
        {
            return await GuardedAsync(async () =>
            {
                await _pollService.RejectAsync(WithUser(args));
                return true;
            });
        }

        [HttpPost]
        [PermissionAuthorize(AdministrationPermissions.Poll)]
        public async Task<IActionResult> Close(int id)
        {
            return await GuardedAsync(async () =>
            {
                await _pollService.CloseAsync(id, GetUserAndOrganization());
                return true;
            });
        }

        [HttpDelete]
        [PermissionAuthorize(AdministrationPermissions.Poll)]
        public async Task<IActionResult> Delete(int id)
        {
            return await GuardedAsync(async () =>
            {
                await _pollService.DeleteAsync(id, GetUserAndOrganization());
                return true;
            });
        }

        [HttpPost]
        [PermissionAuthorize(BasicPermissions.Poll)]
        public async Task<IActionResult> Vote([FromBody] PollVoteDto dto)
        {
            return await GuardedAsync(async () =>
            {
                var userOrg = GetUserAndOrganization();
                dto.UserId = userOrg.UserId;
                dto.OrganizationId = userOrg.OrganizationId;

                await _pollService.VoteAsync(dto);
                return true;
            });
        }

        private PollReviewArgsDto WithUser(PollReviewArgsDto args)
        {
            var userOrg = GetUserAndOrganization();
            args.UserId = userOrg.UserId;
            args.OrganizationId = userOrg.OrganizationId;

            return args;
        }

        private async Task<bool> CanManageAsync(UserAndOrganizationDto userOrg)
        {
            return await _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Poll);
        }

        private async Task<IActionResult> GuardedAsync<T>(Func<Task<T>> action)
        {
            try
            {
                return Ok(await action());
            }
            catch (Exception e) when (e is ArgumentException or InvalidOperationException)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
