using Shrooms.Domain.Services.Search;
using Shrooms.Domain.Services.WebHookCallbacks;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shrooms.Presentation.Api.Controllers
{
    [Route("ExternalJobs")]
    // Authenticated by the Basic filter below, not by JWT: opt out of the fallback policy explicitly.
    [AllowAnonymous]
    [IdentityBasicAuthentication]
    public class ExternalJobsController : BaseController
    {
        private readonly IWebHookCallbackServices _webHookService;
        private readonly ISearchService _searchService;

        public ExternalJobsController(IWebHookCallbackServices webHookService, ISearchService searchService)
        {
            _webHookService = webHookService;
            _searchService = searchService;
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

        // Fills global search after the migration and repairs it nightly: rows changed
        // outside SaveChanges (bulk updates, raw SQL, renamed users) are only picked up here.
        [HttpPost]
        [Route("RebuildSearchIndex")]
        public async Task RebuildSearchIndex()
        {
            await _searchService.RebuildIndexAsync();
        }

        [HttpPost]
        [Route("ProcessExpiredBlacklistUsers")]
        public async Task ProcessExpiredBlacklistUsers()
        {
            await _webHookService.BlacklistUserStatusChange.ProcessExpiredBlacklistUsersAsync();
        }
    }
}
