using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Shrooms.Contracts.Exceptions;
using Shrooms.Premium.DataTransferObjects.Models.Groups;
using Shrooms.Premium.Domain.Services.WebHookCallbacks;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Controllers.Kudos;
using Shrooms.Presentation.Common.Filters;

namespace Shrooms.Premium.Presentation.Api.Controllers
{
    [Route("ExternalPremiumJobs")]
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
        [Route("RemindJoinedEvents")]
        public async Task RemindJoinedEvents()
        {
            await _webHookService.EventRemindService.SendJoinedNotificationsAsync(GetOrganizationName());
        }

        [HttpPost]
        [Route("RemindEvents")]
        public async Task RemindEvents()
        {
            var organizationName = GetOrganizationName();
            await _webHookService.EventRemindService.SendNotificationsAsync(organizationName);
        }

        [HttpPost]
        [Route("ProcessExpiredLotteries")]
        public async Task ProcessExpiredLotteries()
        {
            await _webHookService.LotteryStatusChangeService.ProcessExpiredLotteriesAsync();
        }

        [HttpPost]
        [Route("AwardMonthlyGroupKudos")]
        [ProducesResponseType(typeof(IList<GroupMonthlyKudosResultDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AwardMonthlyGroupKudos(int? year = null, int? month = null)
        {
            try
            {
                var result = await _webHookService.GroupKudos.AwardMonthlyGroupKudosAsync(GetOrganizationName(), year, month);

                return Ok(result);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}
