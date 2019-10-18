using AutoMapper;
using Shrooms.API.Controllers;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.Domain.Services.Lotteries;
using Shrooms.DomainExceptions.Exceptions.Lotteries;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.WebViewModels.Models.Lotteries;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Shrooms.API.Controllers.Lotteries
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
        public IHttpActionResult GetParticipants(int id)
        {
            var participants = _participantService.GetParticipantsCounted(id);
            var viewModel = _mapper.Map<IEnumerable<LotteryParticipantDTO>, IEnumerable<LotteryParticipantViewModel>>(participants);
            return Ok(viewModel);
        }
    }
}