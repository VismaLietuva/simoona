using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.ViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Domain.Services.Lotteries;
using Shrooms.Premium.Presentation.WebViewModels.Lotteries;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Shrooms.Premium.Presentation.Api.Controllers.Lotteries
{
    [Authorize]
    [Route("Lottery")]
    public class LotteryParticipantController : BaseController
    {
        private readonly ILotteryParticipantService _participantService;
        private readonly IMapper _mapper;

        public LotteryParticipantController(ILotteryParticipantService participantService, IMapper mapper)
        {
            _participantService = participantService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("{id}/Participants")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Lottery)]
        [ProducesResponseType(typeof(IEnumerable<LotteryParticipantViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetParticipantsCounted(int id)
        {
            var participants = await _participantService.GetParticipantsCountedAsync(id);
            var viewModel = _mapper.Map<IEnumerable<LotteryParticipantDto>, IEnumerable<LotteryParticipantViewModel>>(participants);

            return Ok(viewModel);
        }

        [HttpGet]
        [Route("Participants/Paged")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Lottery)]
        [ProducesResponseType(typeof(PagedViewModel<LotteryParticipantDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPagedParticipants(int id, int page = 1, int pageSize = WebApiConstants.DefaultPageSize)
        {
            var pagedParticipants = await _participantService.GetPagedParticipantsAsync(id, page, pageSize);
            var pagedModel = new PagedViewModel<LotteryParticipantDto>
            {
                PagedList = pagedParticipants,
                PageCount = pagedParticipants.PageCount,
                ItemCount = pagedParticipants.TotalItemCount,
                PageSize = pagedParticipants.PageSize
            };

            return Ok(pagedModel);
        }
    }
}
