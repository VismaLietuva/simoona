using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.BlacklistUsers;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.BlacklistUsers;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models.BlacklistUsers;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [RoutePrefix("Blacklist")]
    public class BlacklistController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IBlacklistService _blacklistService;

        public BlacklistController(IMapper mapper, IBlacklistService blacklistService)
        {
            _mapper = mapper;
            _blacklistService = blacklistService;
        }

        [HttpPost]
        [Route("")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Blacklist)]
        public async Task<IHttpActionResult> AddToBlacklist(CreateBlacklistUserViewModel createViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createDto = _mapper.Map<CreateBlacklistUserViewModel, CreateBlacklistUserDto>(createViewModel);

                await _blacklistService.CreateAsync(createDto, GetUserAndOrganization());

                return Ok(createViewModel);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [Route("")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Blacklist)]
        public async Task<IHttpActionResult> UpdateBlacklist([FromUri] UpdateBlacklistUserViewModel updateViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updateDto = _mapper.Map<UpdateBlacklistUserViewModel, UpdateBlacklistUserDto>(updateViewModel);

                await _blacklistService.UpdateAsync(updateDto, GetUserAndOrganization());

                return Ok(updateViewModel);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [Route("{id}/Cancel")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Blacklist)]
        public async Task<IHttpActionResult> CancelBlacklist(string id)
        {
            try
            {
                await _blacklistService.CancelAsync(id, GetUserAndOrganization());

                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [Route("{id}")]
        [PermissionAuthorize(Permission = BasicPermissions.Blacklist)]
        public async Task<IHttpActionResult> GetActiveBlacklist(string id)
        {
            var blacklistUserDto = await _blacklistService.GetAsync(id, GetUserAndOrganization());

            if (blacklistUserDto == null)
            {
                return NotFound();
            }

            var blacklistUserViewModel = _mapper.Map<BlacklistUserDto, BlacklistUserViewModel>(blacklistUserDto);

            return Ok(blacklistUserViewModel);
        }

        [HttpGet]
        [Route("{id}/History")]
        public async Task<IHttpActionResult> GetBlacklistHistory(string id)
        {
            try
            {
                var blacklistUserDtos = await _blacklistService.GetAllExceptActiveAsync(id, GetUserAndOrganization());
                var blacklistUserViewModels = _mapper.Map<IEnumerable<BlacklistUserDto>, IEnumerable<BlacklistUserViewModel>>(blacklistUserDtos);

                return Ok(blacklistUserViewModels);
            }
            catch (ValidationException)
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }
        }
    }
}
