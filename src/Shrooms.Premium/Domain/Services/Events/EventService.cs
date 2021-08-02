using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;
using Shrooms.Premium.Domain.Services.Events.Participation;
using Shrooms.Premium.Domain.Services.Events.Utilities;

namespace Shrooms.Premium.Domain.Services.Events
{
    public class EventService : IEventService
    {
        private const int NoOptions = 0;
        private readonly IUnitOfWork2 _uow;
        private readonly IPermissionService _permissionService;
        private readonly IEventUtilitiesService _eventUtilitiesService;
        private readonly IEventValidationService _eventValidationService;
        private readonly IEventParticipationService _eventParticipationService;
        private readonly IWallService _wallService;
        private readonly IMarkdownConverter _markdownConverter;
        private readonly IDbSet<Event> _eventsDbSet;
        private readonly IDbSet<EventType> _eventTypesDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<EventOption> _eventOptionsDbSet;

        private readonly IDbSet<Office> _officeDbSet;

        public EventService(IUnitOfWork2 uow,
                            IPermissionService permissionService,
                            IEventUtilitiesService eventUtilitiesService,
                            IEventValidationService eventValidationService,
                            IEventParticipationService eventParticipationService,
                            IWallService wallService,
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
            _wallService = wallService;
            _markdownConverter = markdownConverter;
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
        }

        public EventEditDTO GetEventForEditing(Guid id, UserAndOrganizationDTO userOrg)
        {
            var @event = _eventsDbSet
                .Include(e => e.ResponsibleUser)
                .Where(e => e.Id == id && e.OrganizationId == userOrg.OrganizationId)
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
                .Where(e => e.Id == id && e.OrganizationId == userOrg.OrganizationId)
                .Select(MapToEventDetailsDto(id))
                .SingleOrDefault();

            _eventValidationService.CheckIfEventExists(@event);

            @event.Offices.OfficeNames = _officeDbSet
                .Where(p => @event.Offices.Value.Contains(SqlFunctions.StringConvert((double)p.Id).Trim()))
                .Select(p => p.Name)
                .ToList();

            _eventValidationService.CheckIfEventExists(@event);
            @event.IsFull = @event.Participants.Count(p => p.AttendStatus == (int)AttendingStatus.Attending) >= @event.MaxParticipants;

            @event.GoingCount = @event.Participants.Count(p => p.AttendStatus == (int)AttendingStatus.Attending);
            @event.MaybeGoingCount = @event.Participants.Count(p => p.AttendStatus == (int)AttendingStatus.MaybeAttending);
            @event.NotGoingCount = @event.Participants.Count(p => p.AttendStatus == (int)AttendingStatus.NotAttending);

            var participating = @event.Participants.FirstOrDefault(p => p.UserId == userOrg.UserId);
            @event.ParticipatingStatus = participating?.AttendStatus ?? (int)AttendingStatus.Idle;

            // If user has permissions - show all participants, otherwise show only current user and his own event options
            if (_permissionService.UserHasPermission(userOrg, BasicPermissions.EventUsers))
            {
                return @event;
            }

            @event.Participants = @event.Participants.Where(p => p.UserId == userOrg.UserId).ToList();

            var userEventOptions = @event.Options.Where(o => o.Participants.Any(p => p.UserId == userOrg.UserId));
            foreach (var userEventOption in userEventOptions)
            {
                userEventOption.Participants = @event.Participants;
            }

            return @event;
        }

        public async Task<CreateEventDto> CreateEvent(CreateEventDto newEventDto)
        {
            newEventDto.MaxOptions = FindOutMaxChoices(newEventDto.NewOptions.Count(), newEventDto.MaxOptions);
            newEventDto.RegistrationDeadlineDate = SetRegistrationDeadline(newEventDto);

            var hasPermissionToPin = _permissionService.UserHasPermission(newEventDto, AdministrationPermissions.Event);
            _eventValidationService.CheckIfUserHasPermissionToPin(newEventDto.IsPinned, currentPinStatus: false, hasPermissionToPin);

            _eventValidationService.CheckIfEventStartDateIsExpired(newEventDto.StartDate);
            _eventValidationService.CheckIfRegistrationDeadlineIsExpired(newEventDto.RegistrationDeadlineDate.Value);
            ValidateEvent(newEventDto);
            _eventValidationService.CheckIfCreatingEventHasInsufficientOptions(newEventDto.MaxOptions, newEventDto.NewOptions.Count());
            _eventValidationService.CheckIfCreatingEventHasNoChoices(newEventDto.MaxOptions, newEventDto.NewOptions.Count());

            var newEvent = await MapNewEvent(newEventDto);

            _eventsDbSet.Add(newEvent);

            MapNewOptions(newEventDto, newEvent);
            await _uow.SaveChangesAsync(newEventDto.UserId);

            newEvent.Description = _markdownConverter.ConvertToHtml(newEvent.Description);

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
            _eventValidationService.CheckIfAttendOptionsAllowedToUpdate(eventDto, eventToUpdate);
            ValidateEvent(eventDto);

            if (eventDto.ResetParticipantList)
            {
                _eventParticipationService.ResetAttendees(eventDto.Id, eventDto);
            }

            UpdateWall(eventToUpdate, eventDto);
            UpdateEventInfo(eventDto, eventToUpdate);
            UpdateEventOptions(eventDto, eventToUpdate);

            _uow.SaveChanges(false);

            eventToUpdate.Description = _markdownConverter.ConvertToHtml(eventToUpdate.Description);
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
                AllowMaybeGoing = e.AllowMaybeGoing,
                AllowNotGoing = e.AllowNotGoing,
                RegistrationDeadlineDate = e.RegistrationDeadline,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                HostUserId = e.ResponsibleUserId,
                // Do not use string interpolation here (EF won't be able to project it to SQL)
                HostUserFullName = e.ResponsibleUser.FirstName + " " + e.ResponsibleUser.LastName,
                TypeId = e.EventTypeId,
                Options = e.EventOptions.Select(o => new EventOptionDTO
                {
                    Id = o.Id,
                    Option = o.Option,
                    Rule = o.Rule
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
                    Option = newOption.Option,
                    Rule = newOption.Rule,
                    EventId = editedEvent.Id,
                    Created = DateTime.UtcNow,
                    CreatedBy = editedEvent.UserId,
                    Modified = DateTime.UtcNow,
                    ModifiedBy = editedEvent.UserId
                };
                _eventOptionsDbSet.Add(option);
            }
        }

        private void MapNewOptions(CreateEventDto newEventDto, Event newEvent)
        {
            if (newEventDto.NewOptions != null)
            {
                foreach (var option in newEventDto.NewOptions)
                {
                    if (option != null)
                    {
                        var newOption = new EventOption
                        {
                            Created = DateTime.UtcNow,
                            CreatedBy = newEventDto.UserId,
                            Modified = DateTime.UtcNow,
                            ModifiedBy = newEventDto.UserId,
                            Option = option.Option,
                            Rule = option.Rule,
                            Event = newEvent
                        };
                        _eventOptionsDbSet.Add(newOption);
                    }
                }
            }
        }

        private async Task<Event> MapNewEvent(CreateEventDto newEventDto)
        {
            var newEvent = new Event
            {
                Created = DateTime.UtcNow,
                CreatedBy = newEventDto.UserId,
                OrganizationId = newEventDto.OrganizationId,
                OfficeIds = JsonConvert.DeserializeObject<string[]>(newEventDto.Offices.Value),
                IsPinned = newEventDto.IsPinned
            };

            var newWall = new CreateWallDto
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

        private static void UpdateEventInfo(CreateEventDto newEventDto, Event newEvent)
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
            newEvent.IsPinned = newEventDto.IsPinned;
            newEvent.AllowMaybeGoing = newEventDto.AllowMaybeGoing;
            newEvent.AllowNotGoing = newEventDto.AllowNotGoing;
        }

        private static Expression<Func<Event, EventDetailsDTO>> MapToEventDetailsDto(Guid eventId)
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
                AllowMaybeGoing = e.AllowMaybeGoing,
                AllowNotGoing = e.AllowNotGoing,
                MaxParticipants = e.MaxParticipants,
                MaxOptions = e.MaxChoices,
                HostUserId = e.ResponsibleUserId,
                WallId = e.WallId,
                HostUserFullName = e.ResponsibleUser.FirstName + " " + e.ResponsibleUser.LastName,
                Options = e.EventOptions.Select(o => new EventDetailsOptionDTO
                {
                    Id = o.Id,
                    Name = o.Option,
                    Participants = o.EventParticipants
                        .Where(x => x.EventId == eventId && x.AttendStatus == (int)AttendingStatus.Attending)
                        .Select(p => new EventDetailsParticipantDTO
                        {
                            Id = p.Id,
                            UserId = p.ApplicationUser == null ? string.Empty : p.ApplicationUserId,
                            FullName = p.ApplicationUser.FirstName + " " + p.ApplicationUser.LastName,
                            ImageName = p.ApplicationUser.PictureId,
                            AttendStatus = p.AttendStatus,
                            AttendComment = p.AttendComment
                        })
                }),
                Participants = e.EventParticipants.Select(p => new EventDetailsParticipantDTO
                {
                    Id = p.Id,
                    UserId = p.ApplicationUser == null ? string.Empty : p.ApplicationUserId,
                    FullName = p.ApplicationUser.FirstName + " " + p.ApplicationUser.LastName,
                    ImageName = p.ApplicationUser.PictureId,
                    AttendStatus = p.AttendStatus,
                    AttendComment = p.AttendComment
                })
            };
        }
    }
}