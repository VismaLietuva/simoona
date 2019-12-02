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
        public void RemindBooks()
        {
            _webHookService.Books.RemindAboutBooks();
        }

        [HttpPost]
        [Route("GiveLoyaltyKudos")]
        public void GiveLoyaltyKudos()
        {
            var organizationName = GetOrganizationName();
            _webHookService.LoyaltyKudos.AwardEmployeesWithKudos(organizationName);

            var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
            cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((KudosController t) => t.GetLastKudosLogRecords()));
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
            _webHookService.EventJoinRemindService.SendNotifications(GetUserAndOrganization());
        }
    }
}
