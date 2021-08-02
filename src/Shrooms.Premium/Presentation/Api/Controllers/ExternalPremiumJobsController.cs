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
            await _webHookService.Events.UpdateRecurringEvents();
        }

        [HttpPost]
        [Route("RemindBooks")]
        public void RemindBooks(int daysBefore)
        {
            _webHookService.Books.RemindAboutBooksAsync(daysBefore);
        }

        [HttpPost]
        [Route("GiveLoyaltyKudos")]
        [InvalidateCacheOutput("GetLastKudosLogRecords", typeof(KudosController))]
        public void GiveLoyaltyKudos()
        {
            var organizationName = GetOrganizationName();
            _webHookService.LoyaltyKudos.AwardEmployeesWithKudos(organizationName);
        }

        [HttpPost]
        [Route("AssignBadges")]
        public async Task AssignBadges()
        {
            await _webHookService.BadgesService.AssignBadgesAsync();
        }

        [HttpPost]
        [Route("RemindEvents")]
        public void RemindEvents()
        {
            var organizationName = GetOrganizationName();
            _webHookService.EventJoinRemindService.SendNotifications(organizationName);
        }
    }
}
