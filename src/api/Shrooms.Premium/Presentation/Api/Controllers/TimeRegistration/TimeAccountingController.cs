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
    [Route("TimeRegistration/Accounting")]
    [PermissionAuthorize(Permission = AdministrationPermissions.TimeRegistration)]
    public class TimeAccountingController : TimeRegistrationControllerBase
    {
        private readonly ITimeAccountingService _accountingService;

        public TimeAccountingController(ITimeAccountingService accountingService)
        {
            _accountingService = accountingService;
        }

        [HttpGet]
        [Route("Customers")]
        [ProducesResponseType(typeof(IEnumerable<TimeCustomerDto>), StatusCodes.Status200OK)]
        public Task<IActionResult> Customers()
        {
            return GuardedAsync(() => _accountingService.GetCustomersAsync(GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("Customers")]
        public Task<IActionResult> SaveCustomer([FromBody] TimeCustomerInputDto input)
        {
            return GuardedAsync(async () => new { Id = await _accountingService.SaveCustomerAsync(input, GetUserAndOrganization()) });
        }

        [HttpDelete]
        [Route("Customers/{id:int}")]
        public Task<IActionResult> DeleteCustomer(int id)
        {
            return GuardedAsync(() => _accountingService.DeleteCustomerAsync(id, GetUserAndOrganization()));
        }

        [HttpGet]
        [Route("Projects")]
        [ProducesResponseType(typeof(IEnumerable<TimeProjectDto>), StatusCodes.Status200OK)]
        public Task<IActionResult> Projects()
        {
            return GuardedAsync(() => _accountingService.GetProjectsAsync(GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("Projects")]
        public Task<IActionResult> SaveProject([FromBody] TimeProjectInputDto input)
        {
            return GuardedAsync(async () => new { Id = await _accountingService.SaveProjectAsync(input, GetUserAndOrganization()) });
        }

        [HttpPut]
        [Route("Projects/{id:int}/Archive")]
        public Task<IActionResult> ArchiveProject(int id, [FromBody] TimeArchiveViewModel model)
        {
            return GuardedAsync(() => _accountingService.SetProjectArchivedAsync(id, model.IsArchived, GetUserAndOrganization()));
        }

        [HttpDelete]
        [Route("Projects/{id:int}")]
        public Task<IActionResult> DeleteProject(int id)
        {
            return GuardedAsync(() => _accountingService.DeleteProjectAsync(id, GetUserAndOrganization()));
        }

        [HttpPost]
        [Route("Tasks")]
        public Task<IActionResult> SaveTask([FromBody] TimeProjectTaskInputDto input)
        {
            return GuardedAsync(async () => new { Id = await _accountingService.SaveTaskAsync(input, GetUserAndOrganization()) });
        }

        [HttpDelete]
        [Route("Tasks/{id:int}")]
        public Task<IActionResult> DeleteTask(int id)
        {
            return GuardedAsync(() => _accountingService.DeleteTaskAsync(id, GetUserAndOrganization()));
        }
    }
}
