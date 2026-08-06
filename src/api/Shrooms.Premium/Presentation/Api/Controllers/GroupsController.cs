using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Exceptions;
using Shrooms.Premium.DataTransferObjects.Models.Groups;
using Shrooms.Premium.Domain.Services.Groups;
using Shrooms.Premium.Presentation.WebViewModels.Groups;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;

namespace Shrooms.Premium.Presentation.Api.Controllers
{
    [Authorize]
    [Route("Groups")]
    public class GroupsController : BaseController
    {
        private readonly IGroupsService _groupsService;
        private readonly IGroupKudosService _groupKudosService;
        private readonly IMapper _mapper;

        public GroupsController(IMapper mapper, IGroupsService groupsService, IGroupKudosService groupKudosService)
        {
            _mapper = mapper;
            _groupsService = groupsService;
            _groupKudosService = groupKudosService;
        }

        [HttpGet]
        [Route("GetAll")]
        [PermissionAuthorize(Permission = BasicPermissions.Groups)]
        [ProducesResponseType(typeof(IEnumerable<GroupViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var groups = await _groupsService.GetAllAsync(GetUserAndOrganization());

            return Ok(_mapper.Map<IEnumerable<GroupDto>, IEnumerable<GroupViewModel>>(groups));
        }

        [HttpGet]
        [Route("Get")]
        [PermissionAuthorize(Permission = BasicPermissions.Groups)]
        [ProducesResponseType(typeof(GroupViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var group = await _groupsService.GetAsync(GetUserAndOrganization(), id);

                return Ok(_mapper.Map<GroupDto, GroupViewModel>(group));
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        /// <summary>
        /// Open to everyone; GroupsService enforces the type's creation policy and decides
        /// whether the group starts approved or pending.
        /// </summary>
        [HttpPost]
        [Route("Post")]
        [PermissionAuthorize(Permission = BasicPermissions.Groups)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Post(GroupPostViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dto = _mapper.Map<GroupPostViewModel, GroupPostDto>(viewModel);
            SetOrganizationAndUser(dto);

            try
            {
                await _groupsService.CreateAsync(dto);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }

        /// <summary>
        /// Editing is open to any group member; GroupsService authorizes admin-or-member
        /// and preserves the kudos fields unless the caller is a kudos administrator.
        /// </summary>
        [HttpPut]
        [Route("Put")]
        [PermissionAuthorize(Permission = BasicPermissions.Groups)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Put(GroupPostViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dto = _mapper.Map<GroupPostViewModel, GroupPostDto>(viewModel);
            SetOrganizationAndUser(dto);

            try
            {
                await _groupsService.UpdateAsync(dto);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }

        /// <summary>
        /// Open to everyone; GroupsService allows administrators, and the creator of a
        /// group that is still awaiting approval.
        /// </summary>
        [HttpDelete]
        [Route("Delete")]
        [PermissionAuthorize(Permission = BasicPermissions.Groups)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _groupsService.DeleteAsync(id, GetUserAndOrganization());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }

        [HttpPost]
        [Route("Approve")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Groups)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                await _groupsService.ApproveAsync(id, GetUserAndOrganization());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }

        /// <summary>
        /// Logic App entry point. Idempotent: re-running for the same period is a no-op.
        /// </summary>
        [HttpPost]
        [Route("AwardMonthlyKudos")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Kudos)]
        [ProducesResponseType(typeof(GroupMonthlyKudosResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> AwardMonthlyKudos(int? year = null, int? month = null)
        {
            var now = DateTime.UtcNow;

            var result = await _groupKudosService.AwardMonthlyKudosAsync(
                GetUserAndOrganization(),
                year ?? now.Year,
                month ?? now.Month);

            return Ok(result);
        }
    }
}
