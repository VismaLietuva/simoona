using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.ViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Domain.Services.Lotteries;
using Shrooms.Premium.Presentation.WebViewModels.Lotteries;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Api.Filters;

namespace Shrooms.Premium.Presentation.Api.Controllers.Lotteries
{
    [Authorize]
    [RoutePrefix("Lottery")]
    public class LotteryParticipantController : BaseController
    {
        private readonly IParticipantService _participantService;
        private readonly IMapper _mapper;

        public LotteryParticipantController(IParticipantService participantService, IMapper mapper)
        {
            _participantService = participantService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("{id}/Participants")]
        public IHttpActionResult GetParticipantsCounted(int id)
        {
            var participants = _participantService.GetParticipantsCounted(id);
            var viewModel = _mapper.Map<IEnumerable<LotteryParticipantDTO>, IEnumerable<LotteryParticipantViewModel>>(participants);

            return Ok(viewModel);
        }

        [HttpGet]
        [Route("Participants/Paged")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Lottery)]
        public IHttpActionResult GetPagedParticipants(int id, int page = 1, int pageSize = WebApiConstants.DefaultPageSize)
        {
            var pagedParticipants = _participantService.GetPagedParticipants(id, page, pageSize);
            var pagedModel = new PagedViewModel<LotteryParticipantDTO>
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