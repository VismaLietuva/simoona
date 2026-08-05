using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Newtonsoft.Json;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.ViewModels;
using Shrooms.Contracts.ViewModels.Wall.Posts;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.OfficeMap;
using Shrooms.Premium.Domain.DomainExceptions.Event;
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
using Shrooms.Presentation.Common.Controllers;
using Shrooms.Presentation.Common.Filters;
using Shrooms.Domain.Extensions;
using Shrooms.Presentation.Common.Controllers.Wall;

namespace Shrooms.Premium.Presentation.Api.Controllers
{
    [Authorize]
    [Route("Events")]
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

        [HttpGet]
        [Route("Recurrences")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public IActionResult GetEventRecurrenceOptions()
        {
            var recurrences = _eventUtilitiesService.GetRecurrenceOptions();
            return Ok(recurrences);
        }

        [HttpGet]
        [Route("Types")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        [ProducesResponseType(typeof(IEnumerable<EventTypeViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventTypes()
        {
            var organizationId = GetUserAndOrganization().OrganizationId;
            var types = await _eventUtilitiesService.GetEventTypesAsync(organizationId);
            var result = _mapper.Map<IEnumerable<EventTypeDto>, IEnumerable<EventTypeViewModel>>(types);
            return Ok(result);
        }

        [HttpGet]
        [Route("Offices")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        [ProducesResponseType(typeof(IEnumerable<EventOfficeViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOffices()
        {
            var offices = await _officeMapService.GetOfficesAsync();
            var result = _mapper.Map<IEnumerable<OfficeDto>, IEnumerable<EventOfficeViewModel>>(offices);
            return Ok(result);
        }

        [HttpGet]
        [Route("")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        [ProducesResponseType(typeof(IEnumerable<EventListItemViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventsFiltered([FromQuery] EventFilteredArgsViewModel filteredArgsViewModel)
        {
            EventFilteredArgsDto filteredArgsDto;

            if (filteredArgsViewModel != null)
            {
                filteredArgsDto = _mapper.Map<EventFilteredArgsViewModel, EventFilteredArgsDto>(filteredArgsViewModel);

                if (int.TryParse(filteredArgsDto.TypeId, out var typeIdParsed))
                {
                    filteredArgsDto.TypeIdParsed = typeIdParsed;
                }

                if (int.TryParse(filteredArgsDto.OfficeId, out var officeIdParsed))
                {
                    filteredArgsDto.OfficeIdParsed = officeIdParsed;
                }
            }
            else
            {
                filteredArgsDto = new EventFilteredArgsDto();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var eventListDtos = await _eventListingService.GetEventsFilteredAsync(filteredArgsDto, GetUserAndOrganization());
                var eventListViewModels = _mapper.Map<IEnumerable<EventListItemDto>, IEnumerable<EventListItemViewModel>>(eventListDtos);

                return Ok(eventListViewModels);
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("Create")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel eventViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createEventDto = _mapper.Map<CreateEventDto>(eventViewModel);
            var offices = eventViewModel.Offices.Select(p => p.ToString()).ToList();

            createEventDto.Offices = new EventOfficesDto { Value = JsonConvert.SerializeObject(offices) };
            SetOrganizationAndUser(createEventDto);

            CreateEventDto createdEvent;

            var userHubDto = GetUserAndOrganizationHub();
            try
            {
                createdEvent = await _eventService.CreateEventAsync(createEventDto);

                _asyncRunner.Run<NewEventNotifier>(async notifier => { await notifier.Notify(createdEvent, userHubDto); }, GetOrganizationName());
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateEvent(UpdateEventViewModel eventViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventDto = _mapper.Map<UpdateEventViewModel, EditEventDto>(eventViewModel);
            eventDto.Offices = new EventOfficesDto { Value = JsonConvert.SerializeObject(eventViewModel.Offices.Select(p => p.ToString()).ToList()) };
            SetOrganizationAndUser(eventDto);

            try
            {
                await _eventService.UpdateEventAsync(eventDto);
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }

        [HttpGet]
        [Route("Search")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        [ProducesResponseType(typeof(PagedViewModel<EventListItemViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] EventSearchOptionsViewModel options)
        {
            if (options == null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            var optionsDto = _mapper.Map<EventSearchOptionsViewModel, EventSearchOptionsDto>(options);
            optionsDto.View = options.View ?? EventTimeFrame.Upcoming;
            var pagedEvents = await _eventListingService.SearchEventsAsync(optionsDto, GetUserAndOrganization());
            var viewModels = _mapper.Map<IEnumerable<EventListItemDto>, IEnumerable<EventListItemViewModel>>(pagedEvents);

            return Ok(pagedEvents.ToPagedViewModel(viewModels, optionsDto));
        }

        [HttpGet]
        [Route("MyEvents")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        [ProducesResponseType(typeof(IEnumerable<EventListItemViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyEvents([FromQuery] MyEventsOptionsViewModel options)
        {
            if (options == null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            int? officeIdNullable = null;

            if (options.OfficeId != "all" && int.TryParse(options.OfficeId, out var officeIdParsed))
            {
                officeIdNullable = officeIdParsed;
            }

            var optionsDto = _mapper.Map<MyEventsOptionsViewModel, MyEventsOptionsDto>(options);

            var myEventsListDto = await _eventListingService.GetMyEventsAsync(optionsDto, GetUserAndOrganization(), officeIdNullable);

            var result = _mapper.Map<IEnumerable<EventListItemDto>, IEnumerable<EventListItemViewModel>>(myEventsListDto);

            return Ok(result);
        }

        [HttpGet]
        [Route("MyFoodTeam")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        [ProducesResponseType(typeof(FoodTeamWidgetViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyFoodTeam()
        {
            var foodTeamDto = await _eventListingService.GetMyFoodTeamAsync(GetUserAndOrganization());
            var result = _mapper.Map<FoodTeamWidgetDto, FoodTeamWidgetViewModel>(foodTeamDto);

            return Ok(result);
        }

        [HttpGet]
        [Route("Options")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        [ProducesResponseType(typeof(EventOptionsViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventOptions(Guid eventId)
        {
            try
            {
                var eventOptionsDto = await _eventListingService.GetEventOptionsAsync(eventId, GetUserAndOrganization());
                var result = _mapper.Map<EventOptionsDto, EventOptionsViewModel>(eventOptionsDto);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ToggleEventPin(Guid eventId)
        {
            try
            {
                await _eventService.ToggleEventPinAsync(eventId);
                return Ok();
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("Download")]
        public async Task<IActionResult> DownloadEvent(Guid eventId)
        {
            try
            {
                var userOrg = GetUserAndOrganization();
                var export = await _calendarService.DownloadEventAsync(eventId, userOrg.OrganizationId);
                return File(export.Content, "text/calendar", export.FileName);
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("AddColleague")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddColleague(EventJoinMultipleViewModel eventJoinModel)
        {
            var eventJoinDto = _mapper.Map<EventJoinMultipleViewModel, EventJoinDto>(eventJoinModel);
            SetOrganizationAndUser(eventJoinDto);

            try
            {
                await _eventParticipationService.AddColleagueAsync(eventJoinDto);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Join(EventJoinViewModel joinOptions)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var optionsDto = _mapper.Map<EventJoinViewModel, EventJoinDto>(joinOptions);
            SetOrganizationAndUser(optionsDto);
            optionsDto.ParticipantIds = new List<string> { optionsDto.UserId };

            try
            {
                await _eventParticipationService.JoinAsync(optionsDto);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAttendStatus(UpdateAttendStatusViewModel updateStatusViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updateAttendStatusDto = _mapper.Map<UpdateAttendStatusViewModel, UpdateAttendStatusDto>(updateStatusViewModel);
            SetOrganizationAndUser(updateAttendStatusDto);

            try
            {
                await _eventParticipationService.UpdateAttendStatusAsync(updateAttendStatusDto);
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
        [ProducesResponseType(typeof(EventDetailsViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventDetails(Guid eventId)
        {
            try
            {
                var eventDto = await _eventService.GetEventDetailsAsync(eventId, GetUserAndOrganization());
                var result = _mapper.Map<EventDetailsDto, EventDetailsViewModel>(eventDto);

                var officesCount = await _officeMapService.GetOfficesCountAsync();
                result.IsForAllOffices = result.OfficesName.Count() == officesCount;

                return Ok(result);
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("GetPagedReportParticipants")]
        [ProducesResponseType(typeof(PagedViewModel<EventParticipantReportViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPagedReportParticipants([FromQuery] EventParticipantsReportListingArgsViewModel reportArgsViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var reportArgsDto = _mapper.Map<EventParticipantsReportListingArgsViewModel, EventParticipantsReportListingArgsDto>(reportArgsViewModel);
                var pagedParticipantsDto = await _eventListingService.GetReportParticipantsAsync(reportArgsDto, GetUserAndOrganization());
                var pagedParticipantsViewModel = _mapper.Map<IEnumerable<EventParticipantReportDto>, IEnumerable<EventParticipantReportViewModel>>(pagedParticipantsDto);

                return Ok(pagedParticipantsDto.ToPagedViewModel(pagedParticipantsViewModel, reportArgsViewModel));
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("GetPagedVisitedReportEvents")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Event)]
        [ProducesResponseType(typeof(PagedViewModel<EventVisitedReportViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPagedVisitedReportEvents([FromQuery] EventParticipantVisitedEventsListingArgsViewModel visitedArgsViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var visitedArgsDto = _mapper.Map<EventParticipantVisitedEventsListingArgsViewModel, EventParticipantVisitedEventsListingArgsDto>(visitedArgsViewModel);
                var pagedVisitedEventsDto = await _eventListingService.GetEventParticipantVisitedReportEventsAsync(visitedArgsDto, GetUserAndOrganization());
                var visitedEventsViewModels = _mapper.Map<IEnumerable<EventVisitedReportDto>, IEnumerable<EventVisitedReportViewModel>>(pagedVisitedEventsDto);

                return Ok(pagedVisitedEventsDto.ToPagedViewModel(visitedEventsViewModels, visitedArgsDto));
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("Update")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        [ProducesResponseType(typeof(EventEditDetailsViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventForUpdate(Guid eventId)
        {
            try
            {
                var eventDto = await _eventService.GetEventForEditingAsync(eventId, GetUserAndOrganization());
                var result = _mapper.Map<EventEditDetailsDto, EventEditDetailsViewModel>(eventDto);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(Guid eventId)
        {
            try
            {
                await _eventService.DeleteAsync(eventId, GetUserAndOrganization());
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Leave(Guid eventId, string leaveComment)
        {
            try
            {
                await _eventParticipationService.LeaveAsync(eventId, GetUserAndOrganization(), leaveComment);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Expel(Guid eventId, string userId)
        {
            try
            {
                await _eventParticipationService.ExpelAsync(eventId, GetUserAndOrganization(), userId);
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
        [ProducesResponseType(typeof(IEnumerable<EventUserSearchResultViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchUser(string s, Guid eventId)
        {
            var searchResultDto = await _eventParticipationService.SearchForEventJoinAutocompleteAsync(eventId, s, GetUserAndOrganization());
            var searchResult = _mapper.Map<IEnumerable<EventUserSearchResultDto>, IEnumerable<EventUserSearchResultViewModel>>(searchResultDto);
            return Ok(searchResult);
        }

        [HttpPut]
        [Route("ResetAttendees")]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetAttendees(Guid eventId)
        {
            try
            {
                await _eventParticipationService.ResetAllAttendeesAsync(eventId, GetUserAndOrganization());
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
        public async Task<IActionResult> Export(Guid eventId)
        {
            try
            {
                var export = await _eventExportService.ExportOptionsAndParticipantsAsync(eventId, GetUserAndOrganization());
                return File(export.Content, "application/vnd.ms-excel", export.FileName);
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Event)]
        [Route("MaxParticipants")]
        public async Task<IActionResult> GetMaxEventParticipants()
        {
            var maxParticipants = await _eventParticipationService.GetMaxParticipantsCountAsync(GetUserAndOrganization());
            return Ok(new { value = maxParticipants });
        }

        [HttpPost]
        [Route("Share")]
        [PermissionAuthorize(Permission = BasicPermissions.Post)]
        [ProducesResponseType(typeof(WallPostViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> ShareEvent(ShareEventViewModel shareEventViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _eventService.CheckIfEventExistsAsync(shareEventViewModel.Id, GetUserAndOrganization().OrganizationId);
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }

            var postModel = _mapper.Map<NewPostDto>(shareEventViewModel);
            SetOrganizationAndUser(postModel);
            var userHubDto = GetUserAndOrganizationHub();

            try
            {
                var createdPost = await _postService.CreateNewPostAsync(postModel);
                _asyncRunner.Run<SharedEventNotifier>(async notifier =>
                    await notifier.NotifyAsync(createdPost, userHubDto),
                    GetOrganizationName());

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSelectedOptions(EventChangeOptionViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var changeOptionsDto = _mapper.Map<EventChangeOptionViewModel, EventChangeOptionsDto>(viewModel);
            SetOrganizationAndUser(changeOptionsDto);

            try
            {
                await _eventParticipationService.UpdateSelectedOptionsAsync(changeOptionsDto);
                return Ok();
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("GetReportEventDetails")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Event)]
        [ProducesResponseType(typeof(EventReportDetailsViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReportEventDetails(Guid eventId)
        {
            try
            {
                var eventReportDetailsDto = await _eventService.GetReportEventDetailsAsync(eventId, GetUserAndOrganization());
                var eventReportDetailsViewModel = _mapper.Map<EventReportDetailsDto, EventReportDetailsViewModel>(eventReportDetailsDto);

                return Ok(eventReportDetailsViewModel);
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("GetEventsByTitle")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Event)]
        [ProducesResponseType(typeof(PagedViewModel<EventDetailsListItemViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventsByTitle([FromQuery] EventReportListingArgsViewModel reportArgsViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var reportArgsDto = _mapper.Map<EventReportListingArgsViewModel, EventReportListingArgsDto>(reportArgsViewModel);
                var eventListItemsPagedDto = await _eventListingService.GetNotStartedEventsFilteredByTitleAsync(reportArgsDto, GetUserAndOrganization());
                var eventListItemsViewModel = _mapper.Map<IEnumerable<EventDetailsListItemDto>, IEnumerable<EventDetailsListItemViewModel>>(eventListItemsPagedDto);

                return Ok(eventListItemsPagedDto.ToPagedViewModel(eventListItemsViewModel, reportArgsDto));
            }
            catch (EventException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
