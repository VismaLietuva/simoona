using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.DataTransferObjects.Models.Wall;
using Shrooms.Domain.Services.Events.Calendar;
using Shrooms.Domain.Services.Events.Participation;
using Shrooms.Domain.Services.Events.Utilities;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.DomainServiceValidators.Validators.Events;
using Shrooms.Premium.Other.Shrooms.Constants.BusinessLayer;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Events;
using Shrooms.EntityModels.Models.Multiwall;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Domain.Helpers;
using Shrooms.Resources.Models.Events;
using Shrooms.Domain.Services.UserService;
using System.Threading;
using System.Resources;
using Shrooms.Resources;
using System.Globalization;
using System.Data.Entity.SqlServer;
using Newtonsoft.Json;

namespace Shrooms.Domain.Services.Events
{
    public class EventService : IEventService
    {
        private const int NoOptions = 0;



        private readonly IUnitOfWork2 _uow;
        private readonly IPermissionService _permissionService;
        private readonly IEventUtilitiesService _eventUtilitiesService;
        private readonly IEventValidationService _eventValidationService;
        private readonly IEventParticipationService _eventParticipationService;
        private readonly IEventCalendarService _calendarService;
        private readonly IWallService _wallService;
        private readonly IUserService _userService;
        private readonly IMarkdownConverter _markdownConverter;
        private readonly IDbSet<Event> _eventsDbSet;
        private readonly IDbSet<EventType> _eventTypesDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<EventOption> _eventOptionsDbSet;
        private readonly ResourceManager _resourceManager;

        private readonly IDbSet<Office> _officeDbSet;

        public EventService(IUnitOfWork2 uow,
                            IPermissionService permissionService,
                            IEventUtilitiesService eventUtilitiesService,
                            IEventValidationService eventValidationService,
                            IEventParticipationService eventParticipationService,
                            IEventCalendarService calendarService,
                            IWallService wallService,
                            IUserService userService,
                            IMarkdownConverter markdownConverter)
        {
            _uow = uow;
            _eventsDbSet = uow.GetDbSet<Event>();
            _eventTypesDbSet = uow.GetDbSet<EventType>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _eventOptionsDbSet = uow.GetDbSet<EventOption>();
            _officeDbSet = uow.GetDbSet<Office>();

            _permissionService = permissionService;
            _eventUtilitiesService = eventUtilitiesService;
            _eventValidationService = eventValidationService;
            _eventParticipationService = eventParticipationService;
            _calendarService = calendarService;
            _wallService = wallService;
            _userService = userService;
            _markdownConverter = markdownConverter;
            _resourceManager = new ResourceManager("Shrooms.Resources.Models.Events.Events", typeof(ResourceUtilities).Assembly);
        }

        public void Delete(Guid id, UserAndOrganizationDTO userOrg)
        {
            var @event = _eventsDbSet
                .Include(e => e.EventOptions)
                .Include(e => e.EventParticipants)
                .SingleOrDefault(e =>
                    e.Id == id &&
                    e.OrganizationId == userOrg.OrganizationId);

            _eventValidationService.CheckIfEventExists(@event);
            var isAdmin = _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Event);
            _eventValidationService.CheckIfUserHasPermission(userOrg.UserId, @event.ResponsibleUserId, isAdmin);
            _eventValidationService.CheckIfEventEndDateIsExpired(@event.EndDate);

            var timestamp = DateTime.UtcNow;
            @event.Modified = timestamp;
            @event.ModifiedBy = userOrg.UserId;

            _eventParticipationService.DeleteByEvent(id, userOrg.UserId);
            _eventUtilitiesService.DeleteByEvent(id, userOrg.UserId);

            _eventsDbSet.Remove(@event);

            _uow.SaveChanges(false);

            _wallService.DeleteWall(@event.WallId, userOrg, WallType.Events);

            _calendarService.DeleteEvent(id, userOrg.OrganizationId);
        }

        public EventEditDTO GetEventForEditing(Guid id, UserAndOrganizationDTO userOrg)
        {
            var @event = _eventsDbSet
                .Include(e => e.ResponsibleUser)
                .Where(e =>
                    e.Id == id &&
                    e.OrganizationId == userOrg.OrganizationId)
                .Select(MapToEventEditDto())
                .SingleOrDefault();

            _eventValidationService.CheckIfEventExists(@event);
            return @event;
        }

        public EventDetailsDTO GetEventDetails(Guid id, UserAndOrganizationDTO userOrg)
        {
            var @event = _eventsDbSet
                .Include(e => e.ResponsibleUser)
                .Include(e => e.EventParticipants.Select(v => v.EventOptions))
                .Where(e =>
                    e.Id == id &&
                    e.OrganizationId == userOrg.OrganizationId)
                .Select(MapToEventDetailsDto(id))
                .SingleOrDefault();

            @event.Offices.OfficeNames = _officeDbSet
                .Where(p => @event.Offices.Value.Contains(SqlFunctions.StringConvert((double)p.Id).Trim()))
                .Select(p => p.Name)
                .ToList();
            _eventValidationService.CheckIfEventExists(@event);
            @event.IsFull = @event.Participants.Count() >= @event.MaxParticipants;
            @event.IsParticipating = @event.Participants.Any(p => p.UserId == userOrg.UserId);
            return @event;
        }

        public async Task<CreateEventDto> CreateEvent(CreateEventDto newEventDto)
        {
            newEventDto.MaxOptions = FindOutMaxChoices(newEventDto.NewOptions.Count(), newEventDto.MaxOptions);
            newEventDto.RegistrationDeadlineDate = SetRegistrationDeadline(newEventDto);

            var hasPermissionToPin = _permissionService.UserHasPermission(newEventDto, AdministrationPermissions.Event);
            _eventValidationService.CheckIfUserHasPermissionToPin(newEventDto.IsPinned, hasPermissionToPin);

            _eventValidationService.CheckIfEventStartDateIsExpired(newEventDto.StartDate);
            _eventValidationService.CheckIfRegistrationDeadlineIsExpired(newEventDto.RegistrationDeadlineDate.Value);
            ValidateEvent(newEventDto);
            _eventValidationService.CheckIfCreatingEventHasInsufficientOptions(newEventDto.MaxOptions, newEventDto.NewOptions.Count());
            _eventValidationService.CheckIfCreatingEventHasNoChoices(newEventDto.MaxOptions, newEventDto.NewOptions.Count());


            if (newEventDto.FoodOption == (int)EventConstants.FoodOptions.Optional)
            {
                var usersCultureCode = _userService.GetApplicationUser(newEventDto.ResponsibleUserId).CultureCode;

                var willEatOption = TranslateEventOptions("WillEat", usersCultureCode);
                var willNotEatOption = TranslateEventOptions("WillNotEat", usersCultureCode);

                newEventDto.NewOptions = new List<string>() { willEatOption, willNotEatOption };
                newEventDto.MaxOptions = 1;
            }

            var newEvent = await MapNewEvent(newEventDto);

            _eventsDbSet.Add(newEvent);

            MapNewOptions(newEventDto, newEvent);
            await _uow.SaveChangesAsync(newEventDto.UserId);

            newEvent.Description = _markdownConverter.ConvertToHtml(newEvent.Description);
            _calendarService.CreateEvent(newEvent, newEventDto.OrganizationId);

            newEventDto.Id = newEvent.Id.ToString();

            return newEventDto;
        }

        public void UpdateEvent(EditEventDTO eventDto)
        {
            var eventToUpdate = _eventsDbSet
                .Include(e => e.EventOptions)
                .Include(e => e.EventParticipants)
                .FirstOrDefault(e => e.Id == eventDto.Id && e.OrganizationId == eventDto.OrganizationId);

            var totalOptionsProvided = eventDto.NewOptions.Count() + eventDto.EditedOptions.Count();
            eventDto.MaxOptions = FindOutMaxChoices(totalOptionsProvided, eventDto.MaxOptions);
            eventDto.RegistrationDeadlineDate = SetRegistrationDeadline(eventDto);

            var hasPermission = _permissionService.UserHasPermission(eventDto, AdministrationPermissions.Event);
            _eventValidationService.CheckIfEventExists(eventToUpdate);

            if (eventToUpdate == null)
            {
                return;
            }

            _eventValidationService.CheckIfUserHasPermission(eventDto.UserId, eventToUpdate.ResponsibleUserId, hasPermission);
            _eventValidationService.CheckIfUserHasPermissionToPin(eventDto.IsPinned, eventToUpdate.IsPinned, hasPermission);
            _eventValidationService.CheckIfCreatingEventHasInsufficientOptions(eventDto.MaxOptions, totalOptionsProvided);
            _eventValidationService.CheckIfCreatingEventHasNoChoices(eventDto.MaxOptions, totalOptionsProvided);
            ValidateEvent(eventDto);

            if (eventDto.ResetParticipantList)
            {
                _eventParticipationService.ResetAttendees(eventDto.Id, eventDto);
            }

            if (eventDto.FoodOption == (int)EventConstants.FoodOptions.Optional && (eventToUpdate.FoodOption == (int)EventConstants.FoodOptions.None || eventToUpdate.FoodOption == null))
            {
                var usersCultureCode = _userService.GetApplicationUser(eventDto.ResponsibleUserId).CultureCode;

                var willEatOption = TranslateEventOptions("WillEat", usersCultureCode);
                var willNotEatOption = TranslateEventOptions("WillNotEat", usersCultureCode);

                eventDto.NewOptions = new List<string>() { willEatOption, willNotEatOption };
                eventDto.MaxOptions = 1;
            }

            UpdateWall(eventToUpdate, eventDto);
            UpdateEventInfo(eventDto, eventToUpdate);
            UpdateEventOptions(eventDto, eventToUpdate);

            _uow.SaveChanges(false);

            eventToUpdate.Description = _markdownConverter.ConvertToHtml(eventToUpdate.Description);
            _calendarService.UpdateEvent(eventToUpdate, eventDto.OrganizationId);
        }

        public void ToggleEventPin(Guid id)
        {
            var @event = _eventsDbSet.Find(id);
            @event.IsPinned = !@event.IsPinned;
            _uow.SaveChanges();
        }

        public void CheckIfEventExists(string eventId, int organizationId)
        {
            var @event = _eventsDbSet.FirstOrDefault(e => e.Id.ToString() == eventId && e.OrganizationId == organizationId);
            _eventValidationService.CheckIfEventExists(@event);
        }

        private static int FindOutMaxChoices(int eventOptionsCount, int maxOptions)
        {
            return eventOptionsCount == NoOptions ? NoOptions : maxOptions;
        }

        private static DateTime SetRegistrationDeadline(CreateEventDto newEventDto)
        {
            return newEventDto.RegistrationDeadlineDate ?? newEventDto.StartDate;
        }

        private static Expression<Func<Event, EventEditDTO>> MapToEventEditDto()
        {
            return e => new EventEditDTO
            {
                Id = e.Id,
                Description = e.Description,
                ImageName = e.ImageName,
                Location = e.Place,
                Offices = new EventOfficesDTO { Value = e.Offices },
                IsPinned = e.IsPinned,
                Name = e.Name,
                MaxOptions = e.MaxChoices,
                MaxParticipants = e.MaxParticipants,
                Recurrence = e.EventRecurring,
                RegistrationDeadlineDate = e.RegistrationDeadline,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                HostUserId = e.ResponsibleUserId,
                HostUserFullName = e.ResponsibleUser.FirstName + " " + e.ResponsibleUser.LastName,
                TypeId = e.EventTypeId,
                FoodOption = e.FoodOption,
                Options = e.EventOptions.Select(o => new EventOptionDTO
                {
                    Id = o.Id,
                    Option = o.Option
                })
            };
        }

        private void UpdateWall(Event currentEvent, EditEventDTO updatedEvent)
        {
            var updateWallDto = new UpdateWallDto
            {
                Id = currentEvent.WallId,
                Description = updatedEvent.Description,
                Logo = updatedEvent.ImageName,
                Name = updatedEvent.Name,
                OrganizationId = updatedEvent.OrganizationId,
                UserId = updatedEvent.UserId
            };
            _wallService.UpdateWall(updateWallDto);

            var responsibleUserChanged = currentEvent.ResponsibleUserId != updatedEvent.ResponsibleUserId;
            var currentHostIsParticipating = currentEvent.EventParticipants.Any(x => x.ApplicationUserId == currentEvent.ResponsibleUserId);
            var newHostIsParticipating = currentEvent.EventParticipants.Any(p => p.ApplicationUserId == updatedEvent.ResponsibleUserId);

            if (!responsibleUserChanged)
            {
                return;
            }

            _wallService.RemoveModerator(currentEvent.WallId, currentEvent.ResponsibleUserId, updatedEvent);
            _wallService.AddModerator(currentEvent.WallId, updatedEvent.ResponsibleUserId, updatedEvent);

            if (!newHostIsParticipating)
            {
                _wallService.JoinLeaveWall(currentEvent.WallId, updatedEvent.ResponsibleUserId, updatedEvent.ResponsibleUserId, updatedEvent.OrganizationId, true);
            }

            if (!currentHostIsParticipating)
            {
                _wallService.JoinLeaveWall(currentEvent.WallId, currentEvent.ResponsibleUserId, currentEvent.ResponsibleUserId, updatedEvent.OrganizationId, true);
            }
        }

        private void ValidateEvent(CreateEventDto eventDto)
        {
            var userExists = _usersDbSet.Any(u => u.Id == eventDto.ResponsibleUserId);
            var eventTypeExists = _eventTypesDbSet.Any(e => e.Id == eventDto.TypeId);

            _eventValidationService.CheckIfEndDateIsGreaterThanStartDate(eventDto.StartDate, eventDto.EndDate);
            _eventValidationService.CheckIfRegistrationDeadlineExceedsStartDate(eventDto.RegistrationDeadlineDate.Value, eventDto.StartDate);
            _eventValidationService.CheckIfResponsibleUserNotExists(userExists);
            _eventValidationService.CheckIfOptionsAreDifferent(eventDto.NewOptions);
            _eventValidationService.CheckIfTypeDoesNotExist(eventTypeExists);
            _eventValidationService.CheckIfFoodOptionalAndOptionsNonExistent(eventDto.NewOptions, eventDto.FoodOption);
        }

        private void UpdateEventOptions(EditEventDTO editedEvent, Event @event)
        {
            foreach (var editedOption in editedEvent.EditedOptions)
            {
                var option = @event.EventOptions.Single(o => o.Id == editedOption.Id);
                option.Option = editedOption.Option;
            }

            var removedOptions = @event.EventOptions.Where(o => !editedEvent.EditedOptions.Select(x => x.Id).Contains(o.Id)).ToList();

            foreach (var option in removedOptions)
            {
                _eventOptionsDbSet.Remove(option);
            }

            foreach (var newOption in editedEvent.NewOptions)
            {
                var option = new EventOption
                {
                    Option = newOption,
                    EventId = editedEvent.Id,
                    Created = DateTime.UtcNow,
                    CreatedBy = editedEvent.UserId,
                    Modified = DateTime.UtcNow,
                    ModifiedBy = editedEvent.UserId,
                };
                _eventOptionsDbSet.Add(option);
            }
        }

        private void MapNewOptions(CreateEventDto newEventDto, Event newEvent)
        {
            if (newEventDto.NewOptions != null)
            {
                foreach (var optionName in newEventDto.NewOptions)
                {
                    if (optionName != null)
                    {
                        var newOption = new EventOption()
                        {
                            Created = DateTime.UtcNow,
                            CreatedBy = newEventDto.UserId,
                            Modified = DateTime.UtcNow,
                            ModifiedBy = newEventDto.UserId,
                            Option = optionName,
                            Event = newEvent
                        };
                        _eventOptionsDbSet.Add(newOption);
                    }
                }
            }
        }

        private string TranslateEventOptions(string eventOption, string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);
            return ResourceUtilities.GetResourceValue(_resourceManager, eventOption, culture);
        }

        private async Task<Event> MapNewEvent(CreateEventDto newEventDto)
        {
            var newEvent = new Event()
            {
                Created = DateTime.UtcNow,
                CreatedBy = newEventDto.UserId,
                OrganizationId = newEventDto.OrganizationId,
                OfficeIds = JsonConvert.DeserializeObject<string[]>(newEventDto.Offices.Value),
                IsPinned = newEventDto.IsPinned
            };

            var newWall = new CreateWallDto()
            {
                Name = newEventDto.Name,
                Logo = newEventDto.ImageName,
                Access = WallAccess.Private,
                Type = WallType.Events,
                ModeratorsIds = new List<string> { newEventDto.ResponsibleUserId },
                MembersIds = new List<string> { newEventDto.ResponsibleUserId },
                UserId = newEventDto.UserId,
                OrganizationId = newEventDto.OrganizationId
            };

            var wallId = await _wallService.CreateNewWall(newWall);
            newEvent.WallId = wallId;
            UpdateEventInfo(newEventDto, newEvent);

            return newEvent;
        }

        private void UpdateEventInfo(CreateEventDto newEventDto, Event newEvent)
        {
            newEvent.Modified = DateTime.UtcNow;
            newEvent.ModifiedBy = newEventDto.UserId;
            newEvent.Description = newEventDto.Description;
            newEvent.Offices = newEventDto.Offices.Value;
            newEvent.EndDate = newEventDto.EndDate;
            newEvent.EventRecurring = newEventDto.Recurrence;
            newEvent.EventTypeId = newEventDto.TypeId;
            newEvent.ImageName = newEventDto.ImageName;
            newEvent.MaxChoices = newEventDto.MaxOptions;
            newEvent.MaxParticipants = newEventDto.MaxParticipants;
            newEvent.Place = newEventDto.Location;
            newEvent.ResponsibleUserId = newEventDto.ResponsibleUserId;
            newEvent.StartDate = newEventDto.StartDate;
            newEvent.Name = newEventDto.Name;
            newEvent.RegistrationDeadline = newEventDto.RegistrationDeadlineDate.Value;
            newEvent.FoodOption = newEventDto.FoodOption;
            newEvent.IsPinned = newEventDto.IsPinned;
        }

        private Expression<Func<Event, EventDetailsDTO>> MapToEventDetailsDto(Guid eventId)
        {
            return e => new EventDetailsDTO
            {
                Id = e.Id,
                Description = e.Description,
                ImageName = e.ImageName,
                Name = e.Name,
                Offices = new EventOfficesDTO { Value = e.Offices },
                IsPinned = e.IsPinned,
                Location = e.Place,
                RegistrationDeadlineDate = e.RegistrationDeadline,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                MaxParticipants = e.MaxParticipants,
                MaxOptions = e.MaxChoices,
                HostUserId = e.ResponsibleUserId,
                WallId = e.WallId,
                FoodOption = e.FoodOption,
                HostUserFullName = e.ResponsibleUser.FirstName + " " + e.ResponsibleUser.LastName,
                Options = e.EventOptions.Select(o => new EventDetailsOptionDTO
                {
                    Id = o.Id,
                    Name = o.Option,
                    Participants = o.EventParticipants
                        .Where(x => x.EventId == eventId)
                        .Select(p => new EventDetailsParticipantDTO
                        {
                            Id = p.Id,
                            UserId = p.ApplicationUser == null ? ConstBusinessLayer.EmptyUserId : p.ApplicationUserId,
                            FullName = p.ApplicationUser.FirstName + " " + p.ApplicationUser.LastName,
                            ImageName = p.ApplicationUser.PictureId,
                        })
                }),
                Participants = e.EventParticipants.Select(p => new EventDetailsParticipantDTO
                {
                    Id = p.Id,
                    UserId = p.ApplicationUser == null ? ConstBusinessLayer.EmptyUserId : p.ApplicationUserId,
                    FullName = p.ApplicationUser.FirstName + " " + p.ApplicationUser.LastName,
                    ImageName = p.ApplicationUser.PictureId,
                })
            };
        }
    }
}