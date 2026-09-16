using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.ViewModels;
using Shrooms.Domain.Extensions;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Domain.Services.Vacations;
using Shrooms.Premium.Presentation.WebViewModels.Vacations;
using Shrooms.Presentation.Common.Filters;

namespace Shrooms.Premium.Presentation.Api.Controllers.Vacations
{
    /// <summary>
    /// Separate from <see cref="VacationsController"/>, which keeps the legacy
    /// actions, because BaseController's "[controller]/[action]" template cannot
    /// express "/Vacations/Requests/{id}/Approve" without an absolute route on
    /// every action. The URLs are the same either way.
    /// </summary>
    [Authorize]
    [Route("Vacations")]
    [PermissionAuthorize(Permission = BasicPermissions.Vacation)]
    public class VacationRequestsController : VacationControllerBase
    {
        private readonly IVacationRequestService _requestService;
        private readonly IVacationRequestListService _listService;
        private readonly IHolidayService _holidayService;

        public VacationRequestsController(
            IVacationRequestService requestService,
            IVacationRequestListService listService,
            IHolidayService holidayService)
        {
            _requestService = requestService;
            _listService = listService;
            _holidayService = holidayService;
        }

        [HttpGet]
        [Route("Balance")]
        [ProducesResponseType(typeof(VacationBalanceDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Balance()
        {
            return Ok(await _requestService.GetBalanceAsync(GetUserAndOrganization()));
        }

        [HttpGet]
        [Route("MyRequests")]
        [ProducesResponseType(typeof(PagedViewModel<VacationRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MyRequests([FromQuery] VacationRequestListingViewModel query)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var args = query.ToArgs(GetUserAndOrganization());
            var page = await _listService.GetMyRequestsAsync(args);

            return Ok(page.ToPagedViewModel(page, args));
        }

        [HttpGet]
        [Route("TeamRequests")]
        [ProducesResponseType(typeof(PagedViewModel<VacationRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> TeamRequests([FromQuery] VacationRequestListingViewModel query)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var args = query.ToArgs(GetUserAndOrganization());
            var page = await _listService.GetTeamRequestsAsync(args);

            return Ok(page.ToPagedViewModel(page, args));
        }

        /// <summary>
        /// Drives the Manage tab. Gated on having direct reports rather than on a
        /// role, since that is what decides whose requests reach you.
        /// </summary>
        [HttpGet]
        [Route("TeamSummary")]
        [ProducesResponseType(typeof(VacationTeamSummaryViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> TeamSummary()
        {
            var userOrg = GetUserAndOrganization();

            return Ok(new VacationTeamSummaryViewModel
            {
                IsManager = await _listService.HasDirectReportsAsync(userOrg),
                PendingCount = await _listService.GetPendingTeamCountAsync(userOrg)
            });
        }

        [HttpGet]
        [Route("TeamMembers")]
        [ProducesResponseType(typeof(IEnumerable<UserAutoCompleteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> TeamMembers(string s)
        {
            return Ok(await _listService.GetTeamMembersForAutocompleteAsync(s, GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("Requests")]
        [ProducesResponseType(typeof(VacationRequestDto), StatusCodes.Status200OK)]
        public Task<IActionResult> Create([FromBody] VacationRequestDraftViewModel model)
        {
            return GuardedAsync(() => _requestService.SubmitAsync(model.ToDto(), GetUserAndOrganization()));
        }

        [HttpPut]
        [Route("Requests/{id:int}")]
        [ProducesResponseType(typeof(VacationRequestDto), StatusCodes.Status200OK)]
        public Task<IActionResult> Edit(int id, [FromBody] VacationRequestDraftViewModel model)
        {
            return GuardedAsync(() => _requestService.EditAsync(id, model.ToDto(), GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("Requests/{id:int}/Cancel")]
        [ProducesResponseType(typeof(VacationRequestDto), StatusCodes.Status200OK)]
        public Task<IActionResult> Cancel(int id)
        {
            return GuardedAsync(() => _requestService.CancelAsync(id, GetUserAndOrganization()));
        }

        [HttpGet]
        [Route("Requests/{id:int}/Review")]
        [ProducesResponseType(typeof(VacationRequestDto), StatusCodes.Status200OK)]
        public Task<IActionResult> ForReview(int id)
        {
            return GuardedAsync(() => _requestService.GetForReviewAsync(id, GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("Requests/{id:int}/Approve")]
        [ProducesResponseType(typeof(VacationRequestDto), StatusCodes.Status200OK)]
        public Task<IActionResult> Approve(int id)
        {
            return GuardedAsync(() => _requestService.ApproveAsync(id, GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("Requests/{id:int}/Reject")]
        [ProducesResponseType(typeof(VacationRequestDto), StatusCodes.Status200OK)]
        public Task<IActionResult> Reject(int id, [FromBody] VacationRejectViewModel model)
        {
            return GuardedAsync(() => _requestService.RejectAsync(id, model?.Reason, GetUserAndOrganization()));
        }

        /// <summary>
        /// The public holidays, so the client can grey them out in its date
        /// pickers. Weekends are not sent: the client already knows what a
        /// Saturday is, and sending them would be ten times the payload to say so.
        ///
        /// Both bounds are optional and inclusive. Unparseable values are ignored
        /// rather than rejected — the whole calendar is a few hundred rows, so the
        /// worst case of a bad parameter is a slightly larger response.
        /// </summary>
        [HttpGet]
        [Route("Holidays")]
        [ProducesResponseType(typeof(IEnumerable<HolidayViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Holidays([FromQuery] string from, [FromQuery] string to)
        {
            var holidays = await _holidayService.GetAsync(
                VacationWireFormat.ParseDay(from),
                VacationWireFormat.ParseDay(to));

            return Ok(holidays.Select(holiday => new HolidayViewModel
            {
                Date = VacationWireFormat.ToDay(holiday.Date),
                Name = holiday.Name
            }));
        }
    }
}
