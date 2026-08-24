using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Exceptions;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Domain.Services.Vacations;
using Shrooms.Premium.Presentation.WebViewModels.Vacations;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Presentation.Common.Helpers;

namespace Shrooms.Premium.Presentation.Api.Controllers
{
    /// <summary>
    /// The two legacy actions kept for the old letter flow: the balance figure
    /// and the Vacation Bot history.
    ///
    /// Requests, review and administration live in VacationRequestsController
    /// and VacationsAdministrationController. The entitlement import moved to
    /// Vacations/Administration/Entitlements/Import, which takes the payslip
    /// date the old Upload action could not — it stamped the upload time, and
    /// the whole balance calculation hangs off that date being the day the
    /// figures were measured.
    /// </summary>
    [Authorize]
    public class VacationsController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IVacationService _vacationService;
        private readonly IVacationHistoryService _vacationHistoryService;

        public VacationsController(IMapper mapper, IVacationService vacationService, IVacationHistoryService vacationHistoryService)
        {
            _mapper = mapper;

            _vacationService = vacationService;
            _vacationHistoryService = vacationHistoryService;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Vacation)]
        [ProducesResponseType(typeof(VacationAvailableDaysViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AvailableDays()
        {
            var availableDaysDto = await _vacationService.GetAvailableDaysAsync(GetUserAndOrganization());
            var availableDaysViewModel = _mapper.Map<VacationAvailableDaysViewModel>(availableDaysDto);

            return Ok(availableDaysViewModel);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Vacation)]
        [ProducesResponseType(typeof(VacationViewModel[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVacationHistory()
        {
            try
            {
                var vacationDtos = await _vacationHistoryService.GetVacationHistoryAsync(User.Identity.GetUserId());
                var vacationModels = _mapper.Map<VacationDto[], VacationViewModel[]>(vacationDtos);
                return Ok(vacationModels);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}
