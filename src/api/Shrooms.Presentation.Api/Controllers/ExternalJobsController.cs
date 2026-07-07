using Shrooms.Domain.Services.WebHookCallbacks;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Shrooms.Presentation.Api.Controllers
{
    [Route("ExternalJobs")]
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
