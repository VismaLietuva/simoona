using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Monitors;
using Shrooms.Contracts.Exceptions;
using Shrooms.Domain.Services.Monitors;
using Shrooms.Presentation.Api.Filters;
using Shrooms.Presentation.WebViewModels.Models.Monitors;

namespace Shrooms.Presentation.Api.Controllers.Monitor
{
    [Authorize]
    [RoutePrefix("Monitor")]
    [FeatureToggle(Infrastructure.FeatureToggle.Features.Monitors)]
    public class MonitorController : BaseController
    {
        private readonly IMonitorService _monitorService;
        private readonly IMapper _mapper;

        public MonitorController(IMonitorService monitorService, IMapper mapper)
        {
            _monitorService = monitorService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("List")]
        [PermissionAuthorize(AdministrationPermissions.Monitor)]
        public async Task<IHttpActionResult> GetMonitorList()
        {
            var monitorsDto = await _monitorService.GetMonitorListAsync(GetUserAndOrganization().OrganizationId);
            var monitorsViewModel = _mapper.Map<IEnumerable<MonitorDto>, IEnumerable<MonitorViewModel>>(monitorsDto);
            return Ok(monitorsViewModel);
        }

        [HttpGet]
        [Route("Details")]
        [PermissionAuthorize(AdministrationPermissions.Monitor)]
        public async Task<IHttpActionResult> GetMonitorDetails(int monitorId)
        {
            try
            {
                var monitorDto = await _monitorService.GetMonitorDetailsAsync(GetUserAndOrganization().OrganizationId, monitorId);
                var monitorViewModel = _mapper.Map<MonitorDto, MonitorViewModel>(monitorDto);
                return Ok(monitorViewModel);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(AdministrationPermissions.Monitor)]
        public async Task<IHttpActionResult> CreateMonitor(CreateMonitorViewModel monitor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newMonitorDto = _mapper.Map<CreateMonitorViewModel, MonitorDto>(monitor);

            try
            {
                await _monitorService.CreateMonitorAsync(newMonitorDto, GetUserAndOrganization());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }

        [HttpPut]
        [Route("Update")]
        [PermissionAuthorize(AdministrationPermissions.Monitor)]
        public async Task<IHttpActionResult> UpdateMonitor(MonitorViewModel monitor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var monitorDto = _mapper.Map<MonitorViewModel, MonitorDto>(monitor);
            try
            {
                await _monitorService.UpdateMonitorAsync(monitorDto, GetUserAndOrganization());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }
    }
}
