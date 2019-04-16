using System.Threading.Tasks;
using System.Web.Http;
using Shrooms.API.Controllers.WebApi;
using Shrooms.API.Filters;
using Shrooms.Domain.Services.WebHookCallbacks;
using WebApi.OutputCache.V2;

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

        [HttpPost]
        [Route("AssignBadges")]
        public async Task AssignBadges()
        {
            await _webHookService.BadgesService.AssignBadgesAsync(GetUserAndOrganization().OrganizationId);
        }
    }
}