﻿using AutoMapper;
using Shrooms.API.Filters;
using Shrooms.API.Hubs;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Domain.Services.Events;
using Shrooms.Domain.Services.Events.Export;
using Shrooms.Domain.Services.Events.List;
using Shrooms.Domain.Services.Events.Participation;
using Shrooms.Domain.Services.Events.Utilities;
using Shrooms.Domain.Services.Wall;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.DomainExceptions.Exceptions.Event;
using Shrooms.WebViewModels.Models.Events;
using Shrooms.WebViewModels.Models.User;
using Shrooms.WebViewModels.Models.Wall.Posts;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Shrooms.DataTransferObjects.Models.OfficeMap;
using Shrooms.Domain.Services.OfficeMap;
using Shrooms.WebViewModels.Models.Notifications;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Notifications;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.API.BackgroundWorkers;
using Shrooms.Premium.Main.PresentationLayer.Shrooms.API.BackgroundWorkers;
using System.Linq;
using Newtonsoft.Json;

namespace Shrooms.API.Controllers.WebApi.EventControllers
{
    [Authorize]
    [RoutePrefix("Event")]
    public class EventController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IEventService _eventService;
        private readonly IEventListingService _eventListingService;
        private readonly IEventUtilitiesService _eventUtilitiesService;
        private readonly IEventParticipationService _eventParticipationService;
        private readonly IEventExportService _eventExportService;
        private readonly IPostService _postService;
        private readonly IOfficeMapService _officeMapService;
        private readonly IAsyncRunner _asyncRunner;


        public EventController(
            IMapper mapper,
            IEventService eventService,
            IEventListingService eventListingService,
            IEventUtilitiesService eventUtilitiesService,
            IEventParticipationService eventParticipationService,
            IEventExportService eventExportService,
            IPostService postService,
            IOfficeMapService officeMapService,
            IAsyncRunner asyncRunner)
        {
            _mapper = mapper;
            _eventService = eventService;
            _eventListingService = eventListingService;
            _eventUtilitiesService = eventUtilitiesService;
            _eventParticipationService = eventParticipationService;
            _eventExportService = eventExportService;
            _postService = postService;
            _officeMapService = officeMapService;
            _asyncRunner = asyncRunner;
        }

        [Route("Recurrences")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult GetEventRecurranceOptions()
        {
            var recurrences = _eventUtilitiesService.GetRecurranceOptions();
            return Ok(recurrences);
        }

        [Route("Types")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult GetEventTypes()
        {
            var organizationId = GetUserAndOrganization().OrganizationId;
            var typeDtos = _eventUtilitiesService.GetEventTypes(organizationId);
            var result = _mapper.Map<IEnumerable<EventTypeDTO>, IEnumerable<EventTypeViewModel>>(typeDtos);
            return Ok(result);
        }

        [Route("Offices")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult GetOffices()
        {
            var officeDtos = _officeMapService.GetOffices();
            var result = _mapper.Map<IEnumerable<OfficeDTO>, IEnumerable<EventOfficeViewModel>>(officeDtos);
            return Ok(result);
        }

        [Route("ByTypeAndOffice")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult GetEventsByTypeAndOffice(string typeId, string officeId)
        {
            int? typeIdNullable = null;
            int? officeIdNullable = null;

            if (typeId != "all" && int.TryParse(typeId, out var typeIdParsed))
            {
                typeIdNullable = typeIdParsed;
            }

            if (officeId != "all" && int.TryParse(officeId, out var officeIdParsed))
            {
                officeIdNullable = officeIdParsed;
            }

            var userOrganization = GetUserAndOrganization();
            var eventsListDto = _eventListingService.GetEventsByTypeAndOffice(userOrganization, typeIdNullable, officeIdNullable);

            var result = _mapper.Map<IEnumerable<EventListItemDTO>, IEnumerable<EventListItemViewModel>>(eventsListDto);
            return Ok(result);
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public async Task<IHttpActionResult> CreateEvent(CreateEventViewModel eventViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createEventDTO = _mapper.Map<CreateEventDto>(eventViewModel);
            createEventDTO.Offices = new EventOfficesDTO { Value = JsonConvert.SerializeObject(eventViewModel.Offices.Select(p => p.ToString()).ToList())};
            SetOrganizationAndUser(createEventDTO);
            var userHubDto = GetUserAndOrganizationHub();
            try
            {
                var createdEvent = await _eventService.CreateEvent(createEventDTO);
                _asyncRunner.Run<NewEventNotifier>(notif =>
                {
                    notif.Notify(createdEvent, userHubDto);
                }, GetOrganizationName());

            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }

        [HttpPut]
        [Route("Update")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult UpdateEvent(UpdateEventViewModel eventViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventDTO = _mapper.Map<UpdateEventViewModel, EditEventDTO>(eventViewModel);
            eventDTO.Offices = new EventOfficesDTO { Value = JsonConvert.SerializeObject(eventViewModel.Offices.Select(p => p.ToString()).ToList())};
            SetOrganizationAndUser(eventDTO);
            try
            {
                _eventService.UpdateEvent(eventDTO);
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }

        [Route("All")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult GetAllEvents()
        {
            var userOrganization = GetUserAndOrganization();
            var allListDto = _eventListingService.GetEventsByType(userOrganization);
            var result = _mapper.Map<IEnumerable<EventListItemDTO>, IEnumerable<EventListItemViewModel>>(allListDto);
            return Ok(result);
        }

        [HttpGet]
        [Route("MyEvents")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult GetMyEvents([FromUri]MyEventsOptionsViewModel options, string officeId)
        {
            int? officeIdNullable = null;

            if (officeId != "all" && int.TryParse(officeId, out var officeIdParsed))
            {
                officeIdNullable = officeIdParsed;
            }

            var optionsDto = _mapper.Map<MyEventsOptionsViewModel, MyEventsOptionsDTO>(options);
            SetOrganizationAndUser(optionsDto);
            var myEventsListDto = _eventListingService.GetMyEvents(optionsDto, officeIdNullable);
            var result = _mapper.Map<IEnumerable<EventListItemDTO>, IEnumerable<EventListItemViewModel>>(myEventsListDto);
            return Ok(result);
        }

        [HttpGet]
        [Route("Options")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult GetEventOptions(Guid eventId)
        {
            try
            {
                var eventOptionsDto = _eventListingService.GetEventOptions(eventId, GetUserAndOrganization());
                var result = _mapper.Map<EventOptionsDTO, EventOptionsViewModel>(eventOptionsDto);
                return Ok(result);
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPatch]
        [Route("Pin")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Event)]
        public IHttpActionResult ToggleEventPin(Guid eventId)
        {
            try
            {
                _eventService.ToggleEventPin(eventId);
                return Ok();
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("AddColleague")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult AddColleague(EventJoinMultipleViewModel eventJoinModel)
        {
            var eventJoinDTO = _mapper.Map<EventJoinMultipleViewModel, EventJoinDTO>(eventJoinModel);
            SetOrganizationAndUser(eventJoinDTO);
            try
            {
                _eventParticipationService.Join(eventJoinDTO);
                return Ok();
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("Join")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult Join(EventJoinViewModel joinOptions)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var optionsDto = _mapper.Map<EventJoinViewModel, EventJoinDTO>(joinOptions);
            SetOrganizationAndUser(optionsDto);
            optionsDto.ParticipantIds = new List<string>() { optionsDto.UserId };

            try
            {
                _eventParticipationService.Join(optionsDto);
                return Ok();
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("Details")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult GetEventDetails(Guid eventId)
        {
            try
            {
                var eventDto = _eventService.GetEventDetails(eventId, GetUserAndOrganization());
                var result = _mapper.Map<EventDetailsDTO, EventDetailsViewModel>(eventDto);
                return Ok(result);
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("Update")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult GetEventForUpdate(Guid eventId)
        {
            try
            {
                var eventDto = _eventService.GetEventForEditing(eventId, GetUserAndOrganization());
                var result = _mapper.Map<EventEditDTO, EventEditViewModel>(eventDto);
                return Ok(result);
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult Delete(Guid eventId)
        {
            try
            {
                _eventService.Delete(eventId, GetUserAndOrganization());
                return Ok();
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Route("Leave")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult Leave(Guid eventId)
        {
            try
            {
                _eventParticipationService.Leave(eventId, GetUserAndOrganization());
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

        [HttpDelete]
        [Route("Expel")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult Expel(Guid eventId, string userId)
        {
            try
            {
                _eventParticipationService.Expel(eventId, GetUserAndOrganization(), userId);
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
        [Route("GetUsersForAutoComplete")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult SearchUser(string s, Guid eventId)
        {
            var searchResultDto = _eventParticipationService.SearchForEventJoinAutocomplete(eventId, s, GetUserAndOrganization());
            var searchResult = _mapper.Map<IEnumerable<EventUserSearchResultDTO>, IEnumerable<EventUserSearchResultViewModel>>(searchResultDto);
            return Ok(searchResult);
        }

        [HttpPut]
        [Route("ResetAttendees")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult ResetAttendees(Guid eventId)
        {
            try
            {
                _eventParticipationService.ResetAttendees(eventId, GetUserAndOrganization());
                return Ok();
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        [Route("Export")]
        public IHttpActionResult Export(Guid eventId)
        {
            try
            {
                var stream = new ByteArrayContent(_eventExportService.ExportOptionsAndParticipants(eventId, GetUserAndOrganization()));
                var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = stream };
                return ResponseMessage(result);
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        [Route("MaxParticipants")]
        public IHttpActionResult GetMaxEventParticipants()
        {
            var maxParticipants = _eventParticipationService.GetMaxParticipantsCount(GetUserAndOrganization());
            return Ok(new { value = maxParticipants });
        }

        [HttpPost]
        [Route("Share")]
        [PermissionAuthorize(Permission = BasicPermissions.Post)]
        public IHttpActionResult ShareEvent(ShareEventViewModel shareEventViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _eventService.CheckIfEventExists(shareEventViewModel.EventId, GetUserAndOrganization().OrganizationId);
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }

            var postModel = _mapper.Map<ShareEventViewModel, NewPostDTO>(shareEventViewModel);
            SetOrganizationAndUser(postModel);
            var userHubDto = GetUserAndOrganizationHub();
            try
            {
                var createdPost = _postService.CreateNewPost(postModel);
                _asyncRunner.Run<SharedEventNotifier>(notif =>
                {
                    notif.Notify(postModel, createdPost, userHubDto);
                }, GetOrganizationName());


                var newPostViewModel = _mapper.Map<WallPostViewModel>(createdPost);

                return Ok(newPostViewModel);
            }
            catch (ValidationException e)
            {
                return BadRequestWithError(e);
            }
        }
    }
}