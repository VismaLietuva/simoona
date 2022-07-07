using System.Threading.Tasks;
using System.Web.Http;
using Shrooms.Premium.Domain.Services.WebHookCallbacks;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Api.Controllers.Kudos;
using Shrooms.Presentation.Api.Filters;
using WebApi.OutputCache.V2;

namespace Shrooms.Premium.Presentation.Api.Controllers
{
    [RoutePrefix("ExternalPremiumJobs")]
    [IdentityBasicAuthentication]
    public class ExternalPremiumJobsController : BaseController
    {
        private readonly IWebHookCallbackPremiumServices _webHookService;

        public ExternalPremiumJobsController(IWebHookCallbackPremiumServices webHookService)
        {
            _webHookService = webHookService;
        }

        [HttpPost]
        [Route("UpdateRecurringEvents")]
        public async Task UpdateRecurringEvents()
        {
            await _webHookService.Events.UpdateRecurringEventsAsync();
        }

        [HttpPost]
        [Route("RemindBooks")]
        public async Task RemindBooks(int daysBefore)
        {
            await _webHookService.Books.RemindAboutBooksAsync(daysBefore);
        }

        [HttpPost]
        [Route("GiveLoyaltyKudos")]
        [InvalidateCacheOutput("GetLastKudosLogRecords", typeof(KudosController))]
        public async Task GiveLoyaltyKudos()
        {
            var organizationName = GetOrganizationName();
            await _webHookService.LoyaltyKudos.AwardEmployeesWithKudosAsync(organizationName);
        }

        [HttpPost]
        [Route("AssignBadges")]
        public async Task AssignBadges()
        {
            await _webHookService.BadgesService.AssignBadgesAsync();
        }

        [HttpPost]
        [Route("RemindEvents")]
        public async Task RemindEvents()
        {
            var organizationName = GetOrganizationName();
            await _webHookService.EventJoinRemindService.SendNotificationsAsync(organizationName);
        }

        [HttpPost]
        [Route("UpdateLotteriesStatus")]
        public async Task UpdateLotteriesStatus()
        {
            await _webHookService.LotteryStatusChangeService.UpdateStartedLotteriesToEndedAsync();
        }
    }
}
