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
    [Route("GroupTypes")]
    public class GroupTypesController : BaseController
    {
        private readonly IGroupTypesService _groupTypesService;
        private readonly IMapper _mapper;

        public GroupTypesController(IMapper mapper, IGroupTypesService groupTypesService)
        {
            _mapper = mapper;
            _groupTypesService = groupTypesService;
        }

        /// <summary>
        /// Open to everyone: the Groups page needs the list to know which types a user
        /// may create, and to render a group's type-driven fields. Kudos configuration
        /// is redacted for anyone without KUDOS_ADMINISTRATION.
        /// </summary>
        [HttpGet]
        [Route("Types")]
        [PermissionAuthorize(Permission = BasicPermissions.Groups)]
        [ProducesResponseType(typeof(IEnumerable<GroupTypeViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGroupTypes()
        {
            var types = await _groupTypesService.GetAllAsync(GetUserAndOrganization());

            return Ok(_mapper.Map<IEnumerable<GroupTypeDto>, IEnumerable<GroupTypeViewModel>>(types));
        }

        [HttpGet]
        [Route("Get")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Groups)]
        [ProducesResponseType(typeof(GroupTypeViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var type = await _groupTypesService.GetAsync(GetOrganizationId(), id);

                return Ok(_mapper.Map<GroupTypeDto, GroupTypeViewModel>(type));
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Groups)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create(CreateGroupTypeViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dto = _mapper.Map<CreateGroupTypeViewModel, CreateGroupTypeDto>(viewModel);
            SetOrganizationAndUser(dto);

            try
            {
                await _groupTypesService.CreateAsync(dto);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }

        [HttpPut]
        [Route("Update")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Groups)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(UpdateGroupTypeViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dto = _mapper.Map<UpdateGroupTypeViewModel, UpdateGroupTypeDto>(viewModel);
            SetOrganizationAndUser(dto);

            try
            {
                await _groupTypesService.UpdateAsync(dto);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }

        [HttpDelete]
        [Route("Delete")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Groups)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _groupTypesService.DeleteAsync(id, GetUserAndOrganization());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }
    }
}
