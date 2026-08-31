using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.ViewModels;
using Shrooms.Domain.Extensions;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Domain.Services.Vacations;
using Shrooms.Premium.Presentation.WebViewModels.Vacations;
using Shrooms.Presentation.Common.Filters;

namespace Shrooms.Premium.Presentation.Api.Controllers.Vacations
{
    [Authorize]
    [Route("Vacations/Administration")]
    [PermissionAuthorize(Permission = AdministrationPermissions.Vacation)]
    public class VacationsAdministrationController : VacationControllerBase
    {
        /// <summary>Guards against a hand-posted request pulling a whole payroll year into memory.</summary>
        private const long MaxImportFileSizeInBytes = 10 * 1024 * 1024;

        private readonly IVacationRequestService _requestService;
        private readonly IVacationRequestListService _listService;
        private readonly IVacationLogService _logService;
        private readonly IVacationStatisticsService _statisticsService;
        private readonly IVacationSettingsService _settingsService;
        private readonly IVacationReportService _reportService;
        private readonly IVacationOrderService _orderService;
        private readonly IVacationService _vacationService;

        public VacationsAdministrationController(
            IVacationRequestService requestService,
            IVacationRequestListService listService,
            IVacationLogService logService,
            IVacationStatisticsService statisticsService,
            IVacationSettingsService settingsService,
            IVacationReportService reportService,
            IVacationOrderService orderService,
            IVacationService vacationService)
        {
            _requestService = requestService;
            _listService = listService;
            _logService = logService;
            _statisticsService = statisticsService;
            _settingsService = settingsService;
            _reportService = reportService;
            _orderService = orderService;
            _vacationService = vacationService;
        }

        [HttpGet]
        [Route("Requests")]
        [ProducesResponseType(typeof(PagedViewModel<VacationRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Requests([FromQuery] VacationRequestListingViewModel query)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var args = query.ToArgs(GetUserAndOrganization());
            var page = await _listService.GetAllRequestsAsync(args);

            return Ok(page.ToPagedViewModel(page, args));
        }

        [HttpPut]
        [Route("Requests/{id:int}")]
        [ProducesResponseType(typeof(VacationRequestDto), StatusCodes.Status200OK)]
        public Task<IActionResult> EditRequest(int id, [FromBody] VacationAdminPatchViewModel model)
        {
            return GuardedAsync(() =>
                _requestService.AdminEditAsync(id, model.ToDto(), GetUserAndOrganization()));
        }

        [HttpGet]
        [Route("Log")]
        [ProducesResponseType(typeof(PagedViewModel<VacationEventDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Log([FromQuery] VacationLogListingViewModel query)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var args = query.ToArgs(GetUserAndOrganization());
            var page = await _logService.GetLogAsync(args);

            return Ok(page.ToPagedViewModel(page, args));
        }

        [HttpGet]
        [Route("Statistics")]
        [ProducesResponseType(typeof(VacationStatisticsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Statistics([FromQuery] VacationStatisticsListingViewModel query)
        {
            var args = (query ?? new VacationStatisticsListingViewModel()).ToArgs(GetUserAndOrganization());

            return Ok(await _statisticsService.GetStatisticsAsync(args));
        }

        [HttpGet]
        [Route("Settings")]
        [ProducesResponseType(typeof(VacationSettingsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Settings()
        {
            return Ok(await _settingsService.GetAsync(GetUserAndOrganization()));
        }

        [HttpPut]
        [Route("Settings")]
        [ProducesResponseType(typeof(VacationSettingsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSettings([FromBody] VacationSettingsViewModel model)
        {
            var dto = (model ?? new VacationSettingsViewModel()).ToDto();

            return Ok(await _settingsService.UpdateAsync(dto, GetUserAndOrganization()));
        }

        /// <summary>
        /// Supersedes the old Upload action, which stamped DateTime.UtcNow.
        /// <paramref name="asOf"/> is the payslip date the figures were measured
        /// at; leave it empty and the export's own preamble is read instead.
        /// </summary>
        [HttpPost]
        [Route("Entitlements/Import")]
        [ProducesResponseType(typeof(VacationEntitlementImportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ImportEntitlements(IFormFile file, [FromForm] string asOf)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (file.Length >= MaxImportFileSizeInBytes)
            {
                return BadRequest("File is too large");
            }

            return await GuardedAsync(async () =>
            {
                using var stream = file.OpenReadStream();
                return await _vacationService.ImportEntitlementsAsync(stream, file.FileName, asOf, GetUserAndOrganization());
            });
        }

        [HttpGet]
        [Route("Export/Report")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<IActionResult> Report([FromQuery] string from, [FromQuery] string to)
        {
            return GuardedFileAsync(() => _reportService.GetReportAsync(from, to, GetUserAndOrganization()));
        }

        /// <summary>The export read back in: a month of approved leave, as granted.</summary>
        [HttpPost]
        [Route("Report/Import")]
        [ProducesResponseType(typeof(VacationReportImportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ImportReport(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (file.Length >= MaxImportFileSizeInBytes)
            {
                return BadRequest("File is too large");
            }

            return await GuardedAsync(async () =>
            {
                using var stream = file.OpenReadStream();
                return await _reportService.ImportAsync(stream, file.FileName, GetUserAndOrganization());
            });
        }

        /// <summary>Both bounds are calendar days; omit them for every order.</summary>
        [HttpGet]
        [Route("Orders")]
        [ProducesResponseType(typeof(IList<VacationOrderDto>), StatusCodes.Status200OK)]
        public Task<IActionResult> Orders([FromQuery] string from, [FromQuery] string to)
        {
            return GuardedAsync(() => _orderService.GetOrdersAsync(from, to, GetUserAndOrganization()));
        }

        /// <summary>One order per start day per type over the period's approved leave.</summary>
        [HttpPost]
        [Route("Orders/Generate")]
        [ProducesResponseType(typeof(VacationOrderGenerationDto), StatusCodes.Status200OK)]
        public Task<IActionResult> GenerateOrders([FromBody] GenerateVacationOrdersViewModel model)
        {
            var period = model ?? new GenerateVacationOrdersViewModel();

            return GuardedAsync(() => _orderService.GenerateAsync(period.From, period.To, GetUserAndOrganization()));
        }

        /// <summary>"word" or "pdf"; anything else, including nothing, means Word.</summary>
        [HttpGet]
        [Route("Orders/Archive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<IActionResult> OrdersArchive([FromQuery] string from, [FromQuery] string to, [FromQuery] string format)
        {
            return GuardedFileAsync(() => _orderService.GetArchiveAsync(from, to, VacationWireFormat.ParseFormat(format), GetUserAndOrganization()));
        }

        /// <summary>"word" or "pdf"; anything else, including nothing, means Word.</summary>
        [HttpGet]
        [Route("Orders/{id:int}/Document")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<IActionResult> OrderDocument(int id, [FromQuery] string format)
        {
            return GuardedFileAsync(() => _orderService.GetOrderDocumentAsync(id, VacationWireFormat.ParseFormat(format), GetUserAndOrganization()));
        }
    }
}
