using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Contracts.Constants;
using Shrooms.Premium.DataTransferObjects.Models.TimeRegistration;
using Shrooms.Premium.Domain.Services.TimeRegistration;
using Shrooms.Premium.Presentation.WebViewModels.TimeRegistration;
using Shrooms.Presentation.Common.Filters;

namespace Shrooms.Premium.Presentation.Api.Controllers.TimeRegistration
{
    [Authorize]
    [Route("TimeRegistration")]
    [PermissionAuthorize(Permission = BasicPermissions.TimeRegistration)]
    public class TimeRegistrationController : TimeRegistrationControllerBase
    {
        private readonly ITimeTableService _timeTableService;
        private readonly ITimePeriodService _periodService;
        private readonly ITimeReportService _reportService;
        private readonly ITimeOvertimeService _overtimeService;

        public TimeRegistrationController(
            ITimeTableService timeTableService,
            ITimePeriodService periodService,
            ITimeReportService reportService,
            ITimeOvertimeService overtimeService)
        {
            _timeTableService = timeTableService;
            _periodService = periodService;
            _reportService = reportService;
            _overtimeService = overtimeService;
        }

        [HttpGet]
        [Route("OvertimeRequests")]
        [ProducesResponseType(typeof(IEnumerable<TimeOvertimeRequestDto>), StatusCodes.Status200OK)]
        public Task<IActionResult> OvertimeRequests([FromQuery] string employeeId)
        {
            return GuardedAsync(() => _overtimeService.GetForEmployeeAsync(employeeId, GetUserAndOrganization()));
        }

        [HttpGet]
        [Route("OvertimeRequests/Team")]
        [ProducesResponseType(typeof(IEnumerable<TimeOvertimeRequestDto>), StatusCodes.Status200OK)]
        public Task<IActionResult> TeamOvertimeRequests()
        {
            return GuardedAsync(() => _overtimeService.GetTeamAsync(GetUserAndOrganization()));
        }

        [HttpGet]
        [Route("OvertimeRequests/{id:int}/Review")]
        [ProducesResponseType(typeof(TimeOvertimeRequestDto), StatusCodes.Status200OK)]
        public Task<IActionResult> ReviewOvertimeRequest(int id)
        {
            return GuardedAsync(() => _overtimeService.GetForReviewAsync(id, GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("OvertimeRequests")]
        public Task<IActionResult> CreateOvertimeRequest([FromBody] TimeOvertimeRequestViewModel model)
        {
            return GuardedAsync(async () => new
            {
                Id = await _overtimeService.CreateAsync(
                    new TimeOvertimeRequestInputDto
                    {
                        EmployeeId = model.EmployeeId,
                        DateFrom = model.DateFrom,
                        DateTo = model.DateTo,
                        MinutesPerDay = model.MinutesPerDay,
                        Reason = model.Reason
                    },
                    GetUserAndOrganization())
            });
        }

        [HttpPost]
        [Route("OvertimeRequests/{id:int}/Approve")]
        public Task<IActionResult> ApproveOvertimeRequest(int id)
        {
            return GuardedAsync(() => _overtimeService.ApproveAsync(id, GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("OvertimeRequests/{id:int}/Reject")]
        public Task<IActionResult> RejectOvertimeRequest(int id, [FromBody] TimeOvertimeRejectViewModel model)
        {
            return GuardedAsync(() => _overtimeService.RejectAsync(id, model.Comment, GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("OvertimeRequests/{id:int}/Cancel")]
        public Task<IActionResult> CancelOvertimeRequest(int id)
        {
            return GuardedAsync(() => _overtimeService.CancelAsync(id, GetUserAndOrganization()));
        }

        [HttpGet]
        [Route("TimeTable")]
        [ProducesResponseType(typeof(TimeTableDto), StatusCodes.Status200OK)]
        public Task<IActionResult> TimeTable([FromQuery] string month, [FromQuery] string employeeId)
        {
            return GuardedAsync(() => _timeTableService.GetTimeTableAsync(month, employeeId, GetUserAndOrganization()));
        }

        [HttpPut]
        [Route("Entries")]
        public Task<IActionResult> Entries([FromBody] TimeEntriesViewModel model)
        {
            return GuardedAsync(() => _timeTableService.SaveEntriesAsync(model.Month, model.EmployeeId, model.Entries, GetUserAndOrganization()));
        }

        [HttpPut]
        [Route("AutoFill")]
        public Task<IActionResult> AutoFill([FromBody] TimeProjectChoiceViewModel model)
        {
            return GuardedAsync(() => _timeTableService.SetAutoFillAsync(model.ProjectId, model.EmployeeId, GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("MyProjects")]
        public Task<IActionResult> AddProject([FromBody] TimeProjectChoiceViewModel model)
        {
            return GuardedAsync(() => _timeTableService.AddProjectAsync(model.ProjectId, model.EmployeeId, GetUserAndOrganization()));
        }

        [HttpDelete]
        [Route("MyProjects/{projectId:int}")]
        public Task<IActionResult> RemoveProject(int projectId, [FromQuery] string employeeId)
        {
            return GuardedAsync(() => _timeTableService.RemoveProjectAsync(projectId, employeeId, GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("Submit")]
        public Task<IActionResult> Submit([FromBody] TimeMonthViewModel model)
        {
            return GuardedAsync(() => _periodService.SubmitAsync(model.Month, GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("Withdraw")]
        public Task<IActionResult> Withdraw([FromBody] TimeMonthViewModel model)
        {
            return GuardedAsync(() => _periodService.WithdrawAsync(model.Month, GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("Approve")]
        public Task<IActionResult> Approve([FromBody] TimeEmployeeMonthViewModel model)
        {
            return GuardedAsync(() => _periodService.ApproveAsync(model.EmployeeId, model.Month, GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("Unapprove")]
        public Task<IActionResult> Unapprove([FromBody] TimeEmployeeMonthViewModel model)
        {
            return GuardedAsync(() => _periodService.UnapproveAsync(model.EmployeeId, model.Month, GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("Lock")]
        [PermissionAuthorize(Permission = AdministrationPermissions.TimeRegistration)]
        public Task<IActionResult> Lock([FromBody] TimeMonthViewModel model)
        {
            return GuardedAsync(() => _periodService.LockAsync(model.Month, GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("Unlock")]
        [PermissionAuthorize(Permission = AdministrationPermissions.TimeRegistration)]
        public Task<IActionResult> Unlock([FromBody] TimeMonthViewModel model)
        {
            return GuardedAsync(() => _periodService.UnlockAsync(model.Month, GetUserAndOrganization()));
        }

        [HttpGet]
        [Route("Team")]
        [ProducesResponseType(typeof(IEnumerable<TimeTeamMemberDto>), StatusCodes.Status200OK)]
        public Task<IActionResult> Team([FromQuery] string month)
        {
            return GuardedAsync(() => _periodService.GetTeamAsync(month, GetUserAndOrganization()));
        }

        [HttpGet]
        [Route("Employees")]
        [PermissionAuthorize(Permission = AdministrationPermissions.TimeRegistration)]
        [ProducesResponseType(typeof(IEnumerable<TimeTeamMemberDto>), StatusCodes.Status200OK)]
        public Task<IActionResult> Employees([FromQuery] string month)
        {
            return GuardedAsync(() => _periodService.GetEmployeesAsync(month, GetUserAndOrganization()));
        }

        [HttpGet]
        [Route("Report")]
        [PermissionAuthorize(Permission = AdministrationPermissions.TimeRegistration)]
        [ProducesResponseType(typeof(TimeReportDto), StatusCodes.Status200OK)]
        public Task<IActionResult> Report([FromQuery] string month)
        {
            return GuardedAsync(() => _reportService.GetReportAsync(month, GetUserAndOrganization()));
        }

        [HttpGet]
        [Route("Manager")]
        [ProducesResponseType(typeof(TimeManagerDto), StatusCodes.Status200OK)]
        public Task<IActionResult> Manager([FromQuery] string employeeId)
        {
            return GuardedAsync(() => _periodService.GetManagerAsync(employeeId, GetUserAndOrganization()));
        }

        [HttpGet]
        [Route("Overview")]
        [ProducesResponseType(typeof(TimeRegistrationOverviewDto), StatusCodes.Status200OK)]
        public Task<IActionResult> Overview()
        {
            return GuardedAsync(() => _periodService.GetOverviewAsync(GetUserAndOrganization()));
        }
    }
}
