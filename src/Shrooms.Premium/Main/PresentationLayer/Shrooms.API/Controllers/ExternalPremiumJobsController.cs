using Shrooms.API.Controllers;
using Shrooms.API.Filters;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.WebHookCallbacks;
using System.Threading.Tasks;
using System.Web.Http;
using Shrooms.API.Controllers.Kudos;
using WebApi.OutputCache.V2;

namespace Shrooms.Premium.Main.PresentationLayer.Shrooms.API.Controllers
{
    [IdentityBasicAuthentication]
    public class ExternalPremiumJobsController : BaseController
    {
        private readonly IWebHookCallbackServices _webHookService;

        public ExternalPremiumJobsController(IWebHookCallbackServices webHookService)
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
        [Route("GiveLoyaltyKudos")]
        public void GiveLoyaltyKudos()
        {
            _webHookService.LoyaltyKudos.AwardEmployeesWithKudos(GetOrganizationName());

            var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
            cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((KudosController t) => t.GetLastKudosLogRecords()));
        }
    }
}
