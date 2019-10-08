using Shrooms.API.Controllers;
using Shrooms.DataLayer.DAL;
using Shrooms.Domain.Services.Lotteries;
using Shrooms.EntityModels.Models.Lotteries;
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

        public LotteryParticipantController(IParticipantService participantService)
        {
            _participantService = participantService;
        }

        [HttpGet]
        [Route("{id}/Participants")]
        public IHttpActionResult GetParticipants(int id)
        {
            return Ok(_participantService.GetParticipantsCounted(id));
        }
    }
}