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
using System.Web.Http;

namespace Shrooms.Premium.Presentation.Api.Controllers.Lotteries
{
    [Authorize]
    [RoutePrefix("Lottery")]
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
        public async Task<IHttpActionResult> GetParticipantsCounted(int id)
        {
            var participants = await _participantService.GetParticipantsCountedAsync(id);
            var viewModel = _mapper.Map<IEnumerable<LotteryParticipantDto>, IEnumerable<LotteryParticipantViewModel>>(participants);

            return Ok(viewModel);
        }

        [HttpGet]
        [Route("Participants/Paged")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Lottery)]
        public async Task<IHttpActionResult> GetPagedParticipants(int id, int page = 1, int pageSize = WebApiConstants.DefaultPageSize)
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
