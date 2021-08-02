using System.Web.Http;
using Shrooms.Domain.Services.WebHookCallbacks;
using Shrooms.Presentation.Api.Filters;

namespace Shrooms.Presentation.Api.Controllers
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
            _webHookService.BirthdaysNotification.SendNotificationsAsync(GetOrganizationName());
        }
    }
}