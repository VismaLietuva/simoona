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

        public TimeRegistrationController(ITimeTableService timeTableService, ITimePeriodService periodService)
        {
            _timeTableService = timeTableService;
            _periodService = periodService;
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
