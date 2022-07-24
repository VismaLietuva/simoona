using System.Collections.Generic;
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
        [Route("Add")]
        [PermissionAuthorize(Permission = AdministrationPermissions.ApplicationUser)]
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
        [Route("Update")]
        [PermissionAuthorize(Permission = AdministrationPermissions.ApplicationUser)]
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
        [Route("Cancel")]
        [PermissionAuthorize(Permission = AdministrationPermissions.ApplicationUser)]
        public async Task<IHttpActionResult> CancelBlacklist(string userId)
        {
            try
            {
                await _blacklistService.CancelAsync(userId, GetUserAndOrganization());

                return Ok();
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [Route("Get")]
        [PermissionAuthorize(Permission = AdministrationPermissions.ApplicationUser)]
        public async Task<IHttpActionResult> GetActiveBlacklist(string userId)
        {
            var blacklistUserDto = await _blacklistService.FindAsync(userId, GetUserAndOrganization());

            if (blacklistUserDto == null)
            {
                return NotFound();
            }

            var blacklistStateViewModel = _mapper.Map<BlacklistUserDto, BlacklistUserViewModel>(blacklistUserDto);

            return Ok(blacklistStateViewModel);
        }

        [HttpGet]
        [Route("History")]
        [PermissionAuthorize(Permission = AdministrationPermissions.ApplicationUser)]
        public async Task<IHttpActionResult> GetBlacklistHistory(string userId)
        {
            var blacklistUserDtos = await _blacklistService.GetAllExceptActiveAsync(userId, GetUserAndOrganization());
            var blacklistUserViewModels = _mapper.Map<IEnumerable<BlacklistUserDto>, IEnumerable<BlacklistUserViewModel>>(blacklistUserDtos);

            return Ok(blacklistUserViewModels);
        }
    }
}