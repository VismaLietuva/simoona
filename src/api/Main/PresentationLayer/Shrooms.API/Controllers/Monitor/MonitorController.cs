using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using Shrooms.API.Filters;
using Shrooms.DataTransferObjects.Models.Monitors;
using Shrooms.Domain.Services.Monitors;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.Exceptions;
using Shrooms.WebViewModels.Models.Monitors;

namespace Shrooms.API.Controllers.Monitor
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
        public IHttpActionResult GetMonitorList()
        {
            var monitorsDTO = _monitorService.GetMonitorList(GetUserAndOrganization().OrganizationId);
            var monitorsViewModel = _mapper.Map<IEnumerable<MonitorDTO>, IEnumerable<MonitorViewModel>>(monitorsDTO);
            return Ok(monitorsViewModel);
        }

        [HttpGet]
        [Route("Details")]
        [PermissionAuthorize(AdministrationPermissions.Monitor)]
        public IHttpActionResult GetMonitorDetails(int monitorId)
        {
            try
            {
                var monitorDTO = _monitorService.GetMonitorDetails(GetUserAndOrganization().OrganizationId, monitorId);
                var monitorViewModel = _mapper.Map<MonitorDTO, MonitorViewModel>(monitorDTO);
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
        public IHttpActionResult CreateMonitor(CreateMonitorViewModel monitor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newMonitorDTO = _mapper.Map<CreateMonitorViewModel, MonitorDTO>(monitor);
            try
            {
                _monitorService.CreateMonitor(newMonitorDTO, GetUserAndOrganization());
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
        public IHttpActionResult UpdateMonitor(MonitorViewModel monitor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var monitorDTO = _mapper.Map<MonitorViewModel, MonitorDTO>(monitor);
            try
            {
                _monitorService.UpdateMonitor(monitorDTO, GetUserAndOrganization());
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }
    }
}
