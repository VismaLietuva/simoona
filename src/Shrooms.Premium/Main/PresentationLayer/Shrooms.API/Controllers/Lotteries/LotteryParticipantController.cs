using AutoMapper;
using Shrooms.API.Controllers;
using Shrooms.DataLayer.DAL;
using Shrooms.Domain.Services.Lotteries;
using Shrooms.DomainExceptions.Exceptions.Lotteries;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Main.PresentationLayer.Shrooms.WebViewModels.Models.Lotteries;
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
            return Ok(_participantService.GetParticipantsCounted(id));
        }

        [HttpPost]
        [Route("Enter")]
        public async Task<IHttpActionResult> BuyLotteryTicket(BuyLotteryTicketViewModel lotteryTickets)
        {
            try
            {
                var buyLotterTicketDTO = _mapper.Map<BuyLotteryTicketViewModel, BuyLotteryTicketDTO>(lotteryTickets);

                await _participantService.BuyLotteryTicketAsync(buyLotterTicketDTO, GetUserAndOrganization());

                return Ok();

            }
            catch (LotteryException ex)
            {

                return BadRequest(ex.Message);
            }
        }
    }
}