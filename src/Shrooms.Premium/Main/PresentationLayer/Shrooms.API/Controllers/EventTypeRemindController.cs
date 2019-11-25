using System.Web.Http;
using Shrooms.API.Controllers;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Notifications;

namespace Shrooms.Premium.Main.PresentationLayer.Shrooms.API.Controllers
{
    [RoutePrefix("EventType")]
    //[IdentityBasicAuthentication]
    public class EventTypeRemindController : BaseController
    {
        private readonly INotificationService _notificationService;

        public EventTypeRemindController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        [Route("Test")]
        public IHttpActionResult TestEndpoint()
        {
            _notificationService.CreateForEventJoinReminder(GetUserAndOrganization());

            return Ok();
        }
    }
}
