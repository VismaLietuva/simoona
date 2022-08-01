using System.Threading.Tasks;
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
        public async Task SendDailyMails()
        {
            await _webHookService.DailyMails.SendDigestedWallPostsAsync();
        }

        [HttpPost]
        [Route("SendBirthdaysNotifications")]
        public async Task SendBirthdaysNotifications()
        {
            await _webHookService.BirthdaysNotification.SendNotificationsAsync(GetOrganizationName());
        }

        [HttpPost]
        [Route("AnonymizeUsers")]
        public async Task AnonymizeUsers()
        {
            await _webHookService.UsersAnonymization.AnonymizeUsersAsync(GetOrganizationName());
        }

        [HttpPost]
        [Route("ProcessExpiredBlacklistUsers")]
        public async Task ProcessExpiredBlacklistUsers()
        {
            await _webHookService.BlacklistUserStatusChange.ProcessExpiredBlacklistUsersAsync();
        }
    }
}