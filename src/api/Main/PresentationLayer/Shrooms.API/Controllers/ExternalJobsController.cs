using System.Web.Http;
using Shrooms.API.Filters;
using Shrooms.Domain.Services.WebHookCallbacks;

namespace Shrooms.API.Controllers
{
    [RoutePrefix("ExternalJobs")]
    [IdentityBasicAuthentication]
    public class ExternalJobsController : BaseController
    {
        private readonly IWebHookCallbackServices _webHookService;

        public ExternalJobsController(IWebHookCallbackServices webHookService)
        {
            _webHookService = webHookService;
        }

        [HttpPost]
        [Route("SendDailyMails")]
        public void SendDailyMails()
        {
            _webHookService.DailyMails.SendDigestedWallPosts();
        }

        [HttpPost]
        [Route("SendBirthdaysNotifications")]
        public void SendBirthdaysNotifications()
        {
            _webHookService.BirthdaysNotification.SendNotifications(GetOrganizationName());
        }
    }
}