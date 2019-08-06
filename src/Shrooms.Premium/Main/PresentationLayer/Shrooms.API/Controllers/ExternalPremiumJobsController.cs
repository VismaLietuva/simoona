using Shrooms.API.Controllers;
using Shrooms.API.Filters;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.WebHookCallbacks;
using System.Threading.Tasks;
using System.Web.Http;
using Shrooms.API.Controllers.Kudos;
using WebApi.OutputCache.V2;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Premium.Main.PresentationLayer.Shrooms.API.BackgroundWorkers;

namespace Shrooms.Premium.Main.PresentationLayer.Shrooms.API.Controllers
{
    [RoutePrefix("ExternalPremiumJobs")]
    [IdentityBasicAuthentication]
    public class ExternalPremiumJobsController : BaseController
    {
        private readonly IWebHookCallbackPremiumServices _webHookService;
        private readonly IAsyncRunner _asyncRunner;

        public ExternalPremiumJobsController(IWebHookCallbackPremiumServices webHookService, IAsyncRunner asyncRunner)
        {
            _webHookService = webHookService;
            _asyncRunner = asyncRunner;
        }

        [HttpPost]
        [Route("UpdateRecurringEvents")]
        public async Task UpdateRecurringEvents()
        {
            await _webHookService.Events.UpdateRecurringEvents();
        }

        [HttpPost]
        [Route("GiveLoyaltyKudos")]
        public void GiveLoyaltyKudos()
        {
            string orgName = GetOrganizationName();
            var awardedEmployees=_webHookService.LoyaltyKudos.AwardEmployeesWithKudos(orgName);
            _asyncRunner.Run<KudosAwardNotifier>(ntf => ntf.Notify(awardedEmployees),orgName);

            var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
            cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((KudosController t) => t.GetLastKudosLogRecords()));
        }

        [HttpPost]
        [Route("AssignBadges")]
        public async Task AssignBadges()
        {
            await _webHookService.BadgesService.AssignBadgesAsync();
        }
    }
}
