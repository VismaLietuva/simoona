using System.Web.Http;
using Shrooms.API.Controllers;
using Shrooms.Domain.Services.WebHookCallbacks;

namespace Shrooms.API.Controllers
{
    [RoutePrefix("EventType")]
    //[IdentityBasicAuthentication]
    public class EventTypeRemindController : BaseController
    {
        private readonly IEventJoinRemindService _remindService;

        public EventTypeRemindController(IEventJoinRemindService remindService)
        {
            _remindService = remindService;
        }

        [HttpPost]
        [Route("Test")]
        public IHttpActionResult TestEndpoint()
        {
            _remindService.Notify(GetUserAndOrganization());
            return Ok();
        }
    }
}
