using System;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.DataTransferObjects.BlacklistStates;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.BlacklistStates;
using Shrooms.Presentation.WebViewModels.Models.BlacklistStates;

namespace Shrooms.Presentation.Api.Controllers
{
    [Authorize]
    [RoutePrefix("BlacklistState")]
    public class BlacklistStateController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IBlacklistStateService _blacklistStateService;

        public BlacklistStateController(IMapper mapper, IBlacklistStateService blacklistStateService)
        {
            _mapper = mapper;
            _blacklistStateService = blacklistStateService;
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IHttpActionResult> CreateBlacklistState(CreateBlacklistStateViewModel createViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createDto = _mapper.Map<CreateBlacklistStateViewModel, CreateBlacklistStateDto>(createViewModel);
                var createdDto = await _blacklistStateService.CreateAsync(createDto, GetUserAndOrganization());
                var createdViewModel = _mapper.Map<BlacklistStateDto, BlacklistStateViewModel>(createdDto);

                return Ok(createdViewModel);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPut]
        [Route("Update")]
        public async Task<IHttpActionResult> UpdateBlacklistState([FromUri] UpdateBlacklistStateViewModel updateViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updateDto = _mapper.Map<UpdateBlacklistStateViewModel, UpdateBlacklistStateDto>(updateViewModel);
                var updatedDto = await _blacklistStateService.UpdateAsync(updateDto, GetUserAndOrganization());
                var updatedViewModel = _mapper.Map<BlacklistStateDto, BlacklistStateViewModel>(updatedDto);

                return Ok(updatedViewModel);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public async Task<IHttpActionResult> DeleteBlacklistState(string userId)
        {
            try
            {
                var deletedDto = await _blacklistStateService.DeleteAsync(userId, GetUserAndOrganization());
                var deletedViewModel = _mapper.Map<BlacklistStateDto, BlacklistStateViewModel>(deletedDto);

                return Ok(deletedViewModel);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}