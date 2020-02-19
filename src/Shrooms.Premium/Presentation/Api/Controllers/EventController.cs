using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Newtonsoft.Json;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.ViewModels.Wall.Posts;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.OfficeMap;
using Shrooms.Premium.Domain.DomainExceptions.Event;
using Shrooms.Premium.Domain.Services.Args;
using Shrooms.Premium.Domain.Services.Events;
using Shrooms.Premium.Domain.Services.Events.Calendar;
using Shrooms.Premium.Domain.Services.Events.Export;
using Shrooms.Premium.Domain.Services.Events.List;
using Shrooms.Premium.Domain.Services.Events.Participation;
using Shrooms.Premium.Domain.Services.Events.Utilities;
using Shrooms.Premium.Domain.Services.OfficeMap;
using Shrooms.Premium.Presentation.Api.BackgroundWorkers;
using Shrooms.Premium.Presentation.WebViewModels.Events;
using Shrooms.Premium.Presentation.WebViewModels.User;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Api.Filters;

namespace Shrooms.Premium.Presentation.Api.Controllers
{
    [Authorize]
    [RoutePrefix("Events")]
    public class EventController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IEventService _eventService;
        private readonly IEventListingService _eventListingService;
        private readonly IEventUtilitiesService _eventUtilitiesService;
        private readonly IEventParticipationService _eventParticipationService;
        private readonly IEventCalendarService _calendarService;
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
            IEventCalendarService calendarService,
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
            _calendarService = calendarService;
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

        [HttpGet]
        [Route("")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult GetEventsFiltered(string typeId = null, string officeId = null, int page = 1,
                                                   DateTime? startDate = null, DateTime? endDate = null)
        {
            var args = new EventsListingFilterArgs
            {
                StartDate = startDate,
                EndDate = endDate,
                IsOnlyMainEvents = typeId == "main",
                Page = page
            };

            if (int.TryParse(typeId, out var typeIdParsed))
            {
                args.TypeId = typeIdParsed;
            }

            if (int.TryParse(officeId, out var officeIdParsed))
            {
                args.OfficeId = officeIdParsed;
            }

            var userOrganization = GetUserAndOrganization();

            try
            {
                var eventsListDto = _eventListingService.GetEventsFiltered(args, userOrganization);
                var result = _mapper.Map<IEnumerable<EventListItemDTO>, IEnumerable<EventListItemViewModel>>(eventsListDto);

                return Ok(result);
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
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
            createEventDTO.Offices = new EventOfficesDTO { Value = JsonConvert.SerializeObject(eventViewModel.Offices.Select(p => p.ToString()).ToList()) };
            SetOrganizationAndUser(createEventDTO);
            CreateEventDto createdEvent;

            var userHubDto = GetUserAndOrganizationHub();
            try
            {
                createdEvent = await _eventService.CreateEvent(createEventDTO);

                _asyncRunner.Run<NewEventNotifier>(notif =>
                {
                    notif.Notify(createdEvent, userHubDto);
                }, GetOrganizationName());
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }

            return Ok(new { createdEvent.Id });
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
            eventDTO.Offices = new EventOfficesDTO { Value = JsonConvert.SerializeObject(eventViewModel.Offices.Select(p => p.ToString()).ToList()) };
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

        [HttpGet]
        [Route("MyEvents")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult GetMyEvents([FromUri] MyEventsOptionsViewModel options, string officeId, int page = 1)
        {
            int? officeIdNullable = null;

            if (officeId != "all" && int.TryParse(officeId, out var officeIdParsed))
            {
                officeIdNullable = officeIdParsed;
            }

            var optionsDto = _mapper.Map<MyEventsOptionsViewModel, MyEventsOptionsDTO>(options);
            SetOrganizationAndUser(optionsDto);
            var myEventsListDto = _eventListingService.GetMyEvents(optionsDto, page, officeIdNullable);
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

        [HttpGet]
        [Route("Download")]
        public IHttpActionResult DownloadEvent(Guid eventId)
        {
            try
            {
                var userOrg = GetUserAndOrganization();
                var stream = new ByteArrayContent(_calendarService.DownloadEvent(eventId, userOrg.OrganizationId));
                var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = stream };
                return ResponseMessage(result);
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
                _eventParticipationService.AddColleague(eventJoinDTO);
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

        [HttpPost]
        [Route("UpdateAttendStatus")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult UpdateAttendStatus(UpdateAttendStatusViewModel updateStatusViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updateAttendStatusDTO = _mapper.Map<UpdateAttendStatusViewModel, UpdateAttendStatusDTO>(updateStatusViewModel);
            SetOrganizationAndUser(updateAttendStatusDTO);
            try
            {
                _eventParticipationService.UpdateAttendStatus(updateAttendStatusDTO);
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
        public IHttpActionResult Leave(Guid eventId, string leaveComment)
        {
            try
            {
                _eventParticipationService.Leave(eventId, GetUserAndOrganization(), leaveComment);
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
                _eventService.CheckIfEventExists(shareEventViewModel.EventsId, GetUserAndOrganization().OrganizationId);
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

        [HttpPost]
        [Route("Options")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IHttpActionResult UpdateSelectedOptions(EventChangeOptionViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var changeOptionsDto = _mapper.Map<EventChangeOptionViewModel, EventChangeOptionsDTO>(viewModel);
            SetOrganizationAndUser(changeOptionsDto);

            try
            {
                _eventParticipationService.UpdateSelectedOptions(changeOptionsDto);
                return Ok();
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}