using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Exceptions;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
using Shrooms.Premium.Domain.Services.Events.Utilities;
using Shrooms.Premium.Presentation.WebViewModels.Events;
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Controllers.Wall;
using Shrooms.Presentation.Common.Filters;
using WebApi.OutputCache.V2;

namespace Shrooms.Premium.Presentation.Api.Controllers
{
    [Authorize]
    [RoutePrefix("EventType")]
    public class EventTypeController : BaseController
    {
        private readonly IEventUtilitiesService _eventUtilitiesService;
        private readonly IMapper _mapper;

        public EventTypeController(IMapper mapper, IEventUtilitiesService eventUtilitiesService)
        {
            _mapper = mapper;
            _eventUtilitiesService = eventUtilitiesService;
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.Event)]
        [Route("Types")]
        public async Task<IHttpActionResult> GetEventTypes()
        {
            var organizationId = GetUserAndOrganization().OrganizationId;
            var types = await _eventUtilitiesService.GetEventTypesAsync(organizationId);
            var result = _mapper.Map<IEnumerable<EventTypeDto>, IEnumerable<EventTypeViewModel>>(types);
            return Ok(result);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = AdministrationPermissions.Event)]
        [Route("Get")]
        public async Task<IHttpActionResult> Get(int id)
        {
            if (id == 0)
            {
                return BadRequest("Invalid request");
            }

            var organizationId = GetUserAndOrganization().OrganizationId;

            try
            {
                var eventTypeDto = await _eventUtilitiesService.GetEventTypeAsync(organizationId, id);
                var eventTypeViewModel = _mapper.Map<EventTypeDto, EventTypeViewModel>(eventTypeDto);

                return Ok(eventTypeViewModel);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Event)]
        public async Task<IHttpActionResult> Create(CreateEventTypeViewModel eventTypeViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventTypeDto = _mapper.Map<CreateEventTypeViewModel, CreateEventTypeDto>(eventTypeViewModel);

            SetOrganizationAndUser(eventTypeDto);

            try
            {
                await _eventUtilitiesService.CreateEventTypeAsync(eventTypeDto);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }

        [HttpPut]
        [Route("Update")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Event)]
        [InvalidateCacheOutput(nameof(WallWidgetsController.Get), typeof(WallWidgetsController))]
        public async Task<IHttpActionResult> Update(UpdateEventTypeViewModel eventTypeViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventTypeDto = _mapper.Map<UpdateEventTypeViewModel, UpdateEventTypeDto>(eventTypeViewModel);

            SetOrganizationAndUser(eventTypeDto);

            try
            {
                await _eventUtilitiesService.UpdateEventTypeAsync(eventTypeDto);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }

            return Ok();
        }

        [HttpDelete]
        [Route("Delete")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Event)]
        public async Task<IHttpActionResult> Delete(int id)
        {
            try
            {
                await _eventUtilitiesService.DeleteEventTypeAsync(id, GetUserAndOrganization());
                return Ok();
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }

        [HttpGet]
        [Route("Groups")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Event)]
        public async Task<IHttpActionResult> GetSingleJoinGroupNames()
        {
            try
            {
                var groups = await _eventUtilitiesService.GetEventTypesSingleJoinGroupsAsync(GetOrganizationId());

                return Ok(groups);
            }
            catch (ArgumentNullException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
