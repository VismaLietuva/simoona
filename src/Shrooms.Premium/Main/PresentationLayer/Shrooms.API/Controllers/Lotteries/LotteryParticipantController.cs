using Shrooms.API.Controllers;
using Shrooms.DataLayer.DAL;
using Shrooms.EntityModels.Models.Lotteries;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Shrooms.Premium.Main.PresentationLayer.Shrooms.API.Controllers.Lotteries
{
    [Authorize]
    [RoutePrefix("Lottery")]
    public class LotteryParticipantController : BaseController
    {
        public LotteryParticipantController()
        {
        }

/*        [HttpGet]
        [Route("test")]
        public IHttpActionResult Test(int lotteryId)
        {
            var par = _participantsDbSet.Where(x => x.LotteryId == lotteryId)
              .GroupBy(l => l.UserId)
              .Select(g => new
              {
                  UserId = g.Key,
                  Count = g.Distinct().Count()
              });

            return Ok(par);
        }*/
    }
}