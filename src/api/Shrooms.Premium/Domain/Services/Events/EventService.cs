using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Events;
using Shrooms.Contracts.DataTransferObjects.Wall;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events.Reminders;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;
using Shrooms.Premium.Domain.Services.Events.Participation;
using Shrooms.Premium.Domain.Services.Events.Utilities;
using Shrooms.Premium.Domain.Services.OfficeMap;

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
        private readonly IOfficeMapService _officeMapService;
        private readonly ISystemClock _systemClock;
        private readonly IEventQuestionWriter _eventQuestionWriter;

        private readonly DbSet<Event> _eventsDbSet;
        private readonly DbSet<EventType> _eventTypesDbSet;
        private readonly DbSet<ApplicationUser> _usersDbSet;
        private readonly DbSet<EventOption> _eventOptionsDbSet;
        private readonly DbSet<EventReminder> _eventRemindersDbSet;
        private readonly DbSet<Office> _officeDbSet;

        public EventService(IUnitOfWork2 uow,
            IPermissionService permissionService,
            IEventUtilitiesService eventUtilitiesService,
            IEventValidationService eventValidationService,
            IEventParticipationService eventParticipationService,
            IWallService wallService,
            IMarkdownConverter markdownConverter,
            IOfficeMapService officeMapService,
            ISystemClock systemClock,
            IEventQuestionWriter eventQuestionWriter)
        {
            _uow = uow;
            _eventsDbSet = uow.GetDbSet<Event>();
            _eventTypesDbSet = uow.GetDbSet<EventType>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _eventOptionsDbSet = uow.GetDbSet<EventOption>();
            _officeDbSet = uow.GetDbSet<Office>();
            _eventRemindersDbSet = uow.GetDbSet<EventReminder>();

            _permissionService = permissionService;
            _eventUtilitiesService = eventUtilitiesService;
            _eventValidationService = eventValidationService;
            _eventParticipationService = eventParticipationService;
            _wallService = wallService;
            _markdownConverter = markdownConverter;
            _officeMapService = officeMapService;
            _systemClock = systemClock;
            _eventQuestionWriter = eventQuestionWriter;
        }

        public async Task DeleteAsync(Guid id, UserAndOrganizationDto userOrg)
        {
            var @event = await _eventsDbSet
                .Include(e => e.EventOptions)
                .Include(e => e.Reminders)
                .Include(e => e.EventParticipants)
                .SingleOrDefaultAsync(e => e.Id == id && e.OrganizationId == userOrg.OrganizationId);

            _eventValidationService.CheckIfEventExists(@event);
            var isAdmin = await _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Event);

            // ReSharper disable once PossibleNullReferenceException
            _eventValidationService.CheckIfUserHasPermission(userOrg.UserId, @event.ResponsibleUserId, isAdmin);
            _eventValidationService.CheckIfEventEndDateIsExpired(@event.EndDate);

            var timestamp = DateTime.UtcNow;
            @event.Modified = timestamp;
            @event.ModifiedBy = userOrg.UserId;

            await _eventParticipationService.DeleteByEventAsync(id, userOrg.UserId);
            await _eventUtilitiesService.DeleteEventOptionsAsync(id, userOrg.UserId);
            await RemoveEventRemindersAsync(@event.Reminders, userOrg.UserId);

            // DeleteEventOptionsAsync takes the question-owned options with it, so leaving the
            // questions behind would strand live rows with every option deleted.
            await _eventQuestionWriter.WriteAsync(id, null, userOrg.UserId);

            _eventsDbSet.Remove(@event);

            await _uow.SaveChangesAsync(false);
            await _wallService.DeleteWallAsync(@event.WallId, userOrg, WallType.Events);
        }

        public async Task<EventEditDetailsDto> GetEventForEditingAsync(Guid id, UserAndOrganizationDto userOrg)
        {
            var @event = await _eventsDbSet
                .Include(e => e.ResponsibleUser)
                .Include(e => e.Reminders)
                .Include(e => e.EventQuestions).ThenInclude(q => q.Options)
                .Where(e => e.Id == id && e.OrganizationId == userOrg.OrganizationId)
                .Select(MapToEventEditDetailsDto())
                .SingleOrDefaultAsync();

            _eventValidationService.CheckIfEventExists(@event);
            return @event;
        }

        public async Task<EventDetailsDto> GetEventDetailsAsync(Guid id, UserAndOrganizationDto userOrg)
        {
            var @event = await _eventsDbSet
                .Include(e => e.ResponsibleUser)
                .Include(e => e.EventParticipants).ThenInclude(v => v.EventOptions)
                .Where(e => e.Id == id && e.OrganizationId == userOrg.OrganizationId)
                .Select(MapToEventDetailsDto(id, userOrg.UserId))
                .SingleOrDefaultAsync();
            _eventValidationService.CheckIfEventExists(@event);

            // ReSharper disable once PossibleNullReferenceException
            @event.Offices.OfficeNames = await _officeDbSet
                .Where(p => @event.Offices.Value.Contains(p.Id.ToString()))
                .Select(p => p.Name)
                .ToListAsync();

            _eventValidationService.CheckIfEventExists(@event);
            @event.IsFull = @event.Participants.Count(p => p.AttendStatus == (int)AttendingStatus.Attending) >= @event.MaxParticipants;

            @event.GoingCount = @event.Participants.Count(p => p.AttendStatus == (int)AttendingStatus.Attending);
            @event.VirtuallyGoingCount = @event.Participants.Count(p => p.AttendStatus == (int)AttendingStatus.AttendingVirtually);
            @event.MaybeGoingCount = @event.Participants.Count(p => p.AttendStatus == (int)AttendingStatus.MaybeAttending);
            @event.NotGoingCount = @event.Participants.Count(p => p.AttendStatus == (int)AttendingStatus.NotAttending);

            var participating = @event.Participants.FirstOrDefault(p => p.UserId == userOrg.UserId);
            @event.ParticipatingStatus = (AttendingStatus?)participating?.AttendStatus ?? AttendingStatus.Idle;

            // If user has permissions - show all participants, otherwise show only current user and his own event options
            if (await _permissionService.UserHasPermissionAsync(userOrg, BasicPermissions.EventUsers))
            {
                return @event;
            }

            @event.Participants = @event.Participants.Where(p => p.UserId == userOrg.UserId).ToList();

            // Every option, not only the ones the caller picked. Rewriting just the caller's own
            // picks left every other option carrying the full participant list — names, pictures
            // and attend comments — for a caller without EventUsers.
            var options = @event.Options.ToList();
            foreach (var option in options)
            {
                option.Participants = option.Participants
                    .Where(p => p.UserId == userOrg.UserId)
                    .ToList();
            }

            @event.Options = options;

            // Each level is materialised: mutating a lazy projection is discarded on re-enumeration.
            var trimmedQuestions = @event.Questions.ToList();
            foreach (var question in trimmedQuestions)
            {
                var questionOptions = question.Options.ToList();
                foreach (var option in questionOptions)
                {
                    option.Participants = option.Participants
                        .Where(p => p.UserId == userOrg.UserId)
                        .ToList();
                }

                question.Options = questionOptions;
            }

            @event.Questions = trimmedQuestions;

            return @event;
        }

        public async Task<CreateEventDto> CreateEventAsync(CreateEventDto newEventDto)
        {
            newEventDto.MaxOptions = FindOutMaxChoices(newEventDto.NewOptions.Count(), newEventDto.MaxOptions);
            newEventDto.RegistrationDeadlineDate = newEventDto.RegistrationDeadlineDate;

            var hasPermissionToPin = await _permissionService.UserHasPermissionAsync(newEventDto, AdministrationPermissions.Event);
            _eventValidationService.CheckIfUserHasPermissionToPin(newEventDto.IsPinned, currentPinStatus: false, hasPermissionToPin);
            _eventValidationService.CheckIfEventStartDateIsExpired(newEventDto.StartDate);
            _eventValidationService.CheckIfRegistrationDeadlineIsExpired(newEventDto.RegistrationDeadlineDate);
            await ValidateEvent(newEventDto);
            _eventValidationService.CheckIfCreatingEventHasInsufficientOptions(newEventDto.MaxOptions, newEventDto.NewOptions.Count());
            _eventValidationService.CheckIfCreatingEventHasNoChoices(newEventDto.MaxOptions, newEventDto.NewOptions.Count());

            // Before the event row is committed: an invalid question tree returned 400 while the
            // event was already persisted, leaving an orphan the caller could not find or retry.
            await _eventQuestionWriter.ValidateAsync(null, newEventDto.Questions);

            var newEvent = await MapNewEventAsync(newEventDto);

            _eventsDbSet.Add(newEvent);

            MapNewOptions(newEventDto, newEvent);
            await _uow.SaveChangesAsync(newEventDto.UserId);

            await _eventQuestionWriter.WriteAsync(newEvent.Id, newEventDto.Questions, newEventDto.UserId);
            await _uow.SaveChangesAsync(newEventDto.UserId);

            newEventDto.Id = newEvent.Id.ToString();

            return newEventDto;
        }

        public async Task<SharedEventEmailDetailsDto> GetSharedEventDetailsAsync(Guid eventId, int organizationId)
        {
            var @event = await _eventsDbSet.Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.Id == eventId && e.OrganizationId == organizationId);
            _eventValidationService.CheckIfEventExists(@event);

            return new SharedEventEmailDetailsDto
            {
                Name = @event.Name,
                StartDate = @event.StartDate,
                TypeName = @event.EventType.Name,
                Description = @event.Description,
                Location = @event.Place
            };
        }

        public async Task UpdateEventAsync(EditEventDto eventDto)
        {
            var eventToUpdate = await _eventsDbSet
                .Include(e => e.EventParticipants).ThenInclude(participant => participant.ApplicationUser).ThenInclude(u => u.Manager)
                .Include(e => e.EventOptions)
                .Include(e => e.EventType)
                .Include(e => e.Reminders)
                .SingleOrDefaultAsync(e => e.Id.ToString() == eventDto.Id && e.OrganizationId == eventDto.OrganizationId);

            var totalOptionsProvided = eventDto.NewOptions.Count() + eventDto.EditedOptions.Count();
            eventDto.MaxOptions = FindOutMaxChoices(totalOptionsProvided, eventDto.MaxOptions);
            eventDto.RegistrationDeadlineDate = eventDto.RegistrationDeadlineDate;

            var hasPermission = await _permissionService.UserHasPermissionAsync(eventDto, AdministrationPermissions.Event);

            _eventValidationService.CheckIfEventExists(eventToUpdate);
            _eventValidationService.CheckIfUserHasPermission(eventDto.UserId, eventToUpdate.ResponsibleUserId, hasPermission);
            _eventValidationService.CheckIfUserHasPermissionToPin(eventDto.IsPinned, eventToUpdate.IsPinned, hasPermission);
            _eventValidationService.CheckIfCreatingEventHasInsufficientOptions(eventDto.MaxOptions, totalOptionsProvided);
            _eventValidationService.CheckIfCreatingEventHasNoChoices(eventDto.MaxOptions, totalOptionsProvided);
            _eventValidationService.CheckIfAttendOptionsAllowedToUpdate(eventDto, eventToUpdate);

            await ValidateEvent(eventDto);

            // With the other rules, before anything mutates: ResetEventAttendessAsync commits the
            // expulsions and sends their emails, so a question tree rejected afterwards would
            // return 400 with the attendee list already wiped.
            await _eventQuestionWriter.ValidateAsync(eventToUpdate.Id, eventDto.Questions);

            await ResetEventAttendessAsync(eventToUpdate, eventDto);
            await UpdateWallAsync(eventToUpdate, eventDto);
            await UpdateEventInfoAsync(eventDto, eventToUpdate);
            UpdateEventOptions(eventDto, eventToUpdate);

            await _eventQuestionWriter.WriteAsync(eventToUpdate.Id, eventDto.Questions, eventDto.UserId);

            await _uow.SaveChangesAsync(false);

            eventToUpdate.Description = _markdownConverter.ConvertToHtml(eventToUpdate.Description);
        }

        private async Task ResetEventAttendessAsync(Event @event, EditEventDto eventDto)
        {
            if (eventDto.ResetParticipantList)
            {
                await _eventParticipationService.ResetAttendeesAsync(@event, eventDto);
            }

            if (eventDto.ResetVirtualParticipantList)
            {
                await _eventParticipationService.ResetVirtualAttendeesAsync(@event, eventDto);
            }
        }

        public async Task ToggleEventPinAsync(Guid id)
        {
            var @event = await _eventsDbSet.FindAsync(id);

            if (@event != null)
            {
                @event.IsPinned = !@event.IsPinned;
                await _uow.SaveChangesAsync();
            }
        }

        public async Task CheckIfEventExistsAsync(string eventId, int organizationId)
        {
            var @event = await _eventsDbSet.FirstOrDefaultAsync(e => e.Id.ToString() == eventId && e.OrganizationId == organizationId);
            _eventValidationService.CheckIfEventExists(@event);
        }

        public async Task<EventReportDetailsDto> GetReportEventDetailsAsync(Guid id, UserAndOrganizationDto userOrg)
        {
            var @event = await _eventsDbSet
                .Include(e => e.ResponsibleUser)
                .Where(e => e.Id == id && e.OrganizationId == userOrg.OrganizationId)
                .Select(e => new EventReportDetailsDto
                {
                    Name = e.Name,
                    ImageName = e.ImageName,
                    Location = e.Place,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    HostUserId = e.ResponsibleUserId,
                    HostUserFullName = e.ResponsibleUser.FirstName + " " + e.ResponsibleUser.LastName,
                    Offices = new EventOfficesDto
                    {
                        Value = e.Offices
                    }
                })
                .SingleOrDefaultAsync();

            _eventValidationService.CheckIfEventExists(@event);

            var officeIds = JsonConvert.DeserializeObject<IEnumerable<int>>(@event.Offices.Value);

            @event.OfficeNames = await _officeDbSet
                .Where(office => officeIds.Contains(office.Id))
                .Select(office => office.Name)
                .ToListAsync();

            @event.IsForAllOffices = (await _officeMapService.GetOfficesCountAsync()) == @event.OfficeNames.Count();

            return @event;
        }

        private static int FindOutMaxChoices(int eventOptionsCount, int maxOptions)
        {
            return eventOptionsCount == NoOptions ? NoOptions : maxOptions;
        }

        private static Expression<Func<Event, EventEditDetailsDto>> MapToEventEditDetailsDto()
        {
            return e => new EventEditDetailsDto
            {
                Id = e.Id,
                Description = e.Description,
                ImageName = e.ImageName,
                Location = e.Place,
                Offices = new EventOfficesDto { Value = e.Offices },
                IsPinned = e.IsPinned,
                Name = e.Name,
                MaxOptions = e.MaxChoices,
                MaxParticipants = e.MaxParticipants,
                MaxVirtualParticipants = e.MaxVirtualParticipants,
                Recurrence = e.EventRecurring,
                AllowMaybeGoing = e.AllowMaybeGoing,
                AllowNotGoing = e.AllowNotGoing,
                RegistrationDeadlineDate = e.RegistrationDeadline,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                HostUserId = e.ResponsibleUserId,
                IsShownInUpcomingEventsWidget = e.IsShownInUpcomingEventsWidget,
                // Do not use string interpolation here (EF won't be able to project it to SQL)
                HostUserFullName = e.ResponsibleUser.FirstName + " " + e.ResponsibleUser.LastName,
                TypeId = e.EventTypeId,
                Reminders = e.Reminders.Select(reminder => new EventReminderDetailsDto
                {
                    RemindBeforeInDays = reminder.RemindBeforeInDays,
                    Type = reminder.Type,
                    RemindedCount = reminder.RemindedCount
                }),
                // Legacy flat options only. Question-owned options are returned under Questions;
                // leaving them here also leaks them into the edit payload's editedOptions, which
                // inflates totalOptionsProvided and trips CheckIfCreatingEventHasNoChoices on any
                // question-bearing event. Mirrors EventListingService.MapOptionsToDto().
                Options = e.EventOptions
                    .Where(o => o.QuestionId == null)
                    .Select(o => new EventOptionDto
                    {
                        Id = o.Id,
                        Option = o.Option,
                        Rule = o.Rule
                    }),
                Questions = e.EventQuestions
                    .OrderBy(q => q.Order)
                    .Select(q => new EventQuestionStructureDto
                    {
                        Id = q.Id,
                        Title = q.Title,
                        Order = q.Order,
                        SelectType = q.SelectType,
                        IsRequired = q.IsRequired,
                        ShowIfOptionId = q.ShowIfOptionId,
                        Options = q.Options
                            .OrderBy(o => o.Order)
                            .Select(o => new EventQuestionOptionStructureDto
                            {
                                Id = o.Id,
                                Name = o.Option,
                                Order = o.Order,
                                Rule = o.Rule
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }

        private async Task UpdateWallAsync(Event currentEvent, EditEventDto updatedEvent)
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

            await _wallService.UpdateWallAsync(updateWallDto);

            var responsibleUserChanged = currentEvent.ResponsibleUserId != updatedEvent.ResponsibleUserId;
            var currentHostIsParticipating = currentEvent.EventParticipants.Any(x => x.ApplicationUserId == currentEvent.ResponsibleUserId);
            var newHostIsParticipating = currentEvent.EventParticipants.Any(p => p.ApplicationUserId == updatedEvent.ResponsibleUserId);

            if (!responsibleUserChanged)
            {
                return;
            }

            await _wallService.RemoveModeratorAsync(currentEvent.WallId, currentEvent.ResponsibleUserId, updatedEvent);
            await _wallService.AddModeratorAsync(currentEvent.WallId, updatedEvent.ResponsibleUserId, updatedEvent);

            if (!newHostIsParticipating)
            {
                await _wallService.JoinOrLeaveWallAsync(currentEvent.WallId, updatedEvent.ResponsibleUserId, updatedEvent.ResponsibleUserId, updatedEvent.OrganizationId, true);
            }

            if (!currentHostIsParticipating)
            {
                await _wallService.JoinOrLeaveWallAsync(currentEvent.WallId, currentEvent.ResponsibleUserId, currentEvent.ResponsibleUserId, updatedEvent.OrganizationId, true);
            }
        }

        private async Task ValidateEvent(IEventArgsDto eventDto)
        {
            var userExists = await _usersDbSet.AnyAsync(u => u.Id == eventDto.ResponsibleUserId);
            var eventTypeExists = await _eventTypesDbSet.AnyAsync(e => e.Id == eventDto.TypeId);

            _eventValidationService.CheckIfEndDateIsGreaterThanStartDate(eventDto.StartDate, eventDto.EndDate);

            // ReSharper disable once PossibleInvalidOperationException
            _eventValidationService.CheckIfRegistrationDeadlineExceedsStartDate(eventDto.RegistrationDeadlineDate, eventDto.StartDate);
            _eventValidationService.CheckIfResponsibleUserNotExists(userExists);
            _eventValidationService.CheckIfOptionsAreDifferent(eventDto.NewOptions);
            _eventValidationService.CheckIfTypeDoesNotExist(eventTypeExists);
        }

        private void UpdateEventOptions(EditEventDto editedEvent, Event @event)
        {
            // @event.EventOptions also holds question-owned options (QuestionId != null), which
            // belong solely to EventQuestionWriter. EditedOptions only ever carries legacy
            // options (see MapToEventEditDetailsDto), so scoping both the edit loop and the
            // removal sweep to QuestionId == null keeps this method from treating every absent
            // question option as "removed" and hard-deleting it.
            var legacyOptions = @event.EventOptions.Where(o => o.QuestionId == null).ToList();

            foreach (var editedOption in editedEvent.EditedOptions)
            {
                var option = legacyOptions.Single(o => o.Id == editedOption.Id);
                option.Option = editedOption.Option;
            }

            var removedOptions = legacyOptions.Where(o => !editedEvent.EditedOptions.Select(x => x.Id).Contains(o.Id)).ToList();

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
                    EventId = Guid.Parse(editedEvent.Id),
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
            if (newEventDto.NewOptions == null)
            {
                return;
            }

            foreach (var option in newEventDto.NewOptions)
            {
                if (option == null)
                {
                    continue;
                }

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

        private async Task<Event> MapNewEventAsync(CreateEventDto newEventDto)
        {
            var newEvent = new Event
            {
                Created = DateTime.UtcNow,
                CreatedBy = newEventDto.UserId,
                OrganizationId = newEventDto.OrganizationId,
                OfficeIds = JsonConvert.DeserializeObject<string[]>(newEventDto.Offices.Value),
                IsPinned = newEventDto.IsPinned,
                Reminders = new List<EventReminder>()
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

            var wallId = await _wallService.CreateNewWallAsync(newWall);
            newEvent.WallId = wallId;
            await UpdateEventInfoAsync(newEventDto, newEvent);

            return newEvent;
        }

        private async Task UpdateEventInfoAsync(IEventArgsDto eventArgsDto, Event newEvent)
        {
            await SetEventRemindersAsync(eventArgsDto, newEvent);
            SetEventInfo(eventArgsDto, newEvent);
        }

        private static void SetEventInfo(IEventArgsDto eventArgsDto, Event newEvent)
        {
            newEvent.Modified = DateTime.UtcNow;
            newEvent.ModifiedBy = eventArgsDto.UserId;
            newEvent.Description = eventArgsDto.Description;
            newEvent.Offices = eventArgsDto.Offices.Value;
            newEvent.EndDate = eventArgsDto.EndDate;
            newEvent.EventRecurring = eventArgsDto.Recurrence;
            newEvent.EventTypeId = eventArgsDto.TypeId;
            newEvent.ImageName = eventArgsDto.ImageName;
            newEvent.MaxChoices = eventArgsDto.MaxOptions;
            newEvent.MaxParticipants = eventArgsDto.MaxParticipants;
            newEvent.MaxVirtualParticipants = eventArgsDto.MaxVirtualParticipants;
            newEvent.Place = eventArgsDto.Location;
            newEvent.ResponsibleUserId = eventArgsDto.ResponsibleUserId;
            newEvent.StartDate = eventArgsDto.StartDate;
            newEvent.Name = eventArgsDto.Name;
            newEvent.IsShownInUpcomingEventsWidget = eventArgsDto.IsShownInUpcomingEventsWidget;

            // ReSharper disable once PossibleInvalidOperationException
            newEvent.RegistrationDeadline = eventArgsDto.RegistrationDeadlineDate;

            newEvent.IsPinned = eventArgsDto.IsPinned;
            newEvent.AllowMaybeGoing = eventArgsDto.AllowMaybeGoing;
            newEvent.AllowNotGoing = eventArgsDto.AllowNotGoing;
        }

        private async Task SetEventRemindersAsync(IEventArgsDto eventArgsDto, Event eventToUpdate)
        {
            var updateReminders = eventToUpdate.Reminders.Join(
                eventArgsDto.Reminders,
                oldReminder => oldReminder.Type,
                reminder => reminder.Type,
                (oldReminder, reminder) => new { OldReminder = oldReminder, Reminder = reminder })
                .ToList();
            foreach (var updateReminder in updateReminders)
            {
                UpdateEventReminder(updateReminder.Reminder, updateReminder.OldReminder, eventArgsDto, eventToUpdate);
            }

            var newReminders = eventArgsDto.Reminders.Where(reminder =>
                !updateReminders.Any(updateReminder => updateReminder.Reminder.Type == reminder.Type) &&
                (eventArgsDto.StartDate != eventArgsDto.RegistrationDeadlineDate || reminder.Type != EventReminderType.Deadline));
            AddEventReminders(eventToUpdate, eventArgsDto, newReminders, eventArgsDto.UserId);

            var remindersToDelete = eventToUpdate.Reminders.Where(reminder =>
                !newReminders.Any(newReminder => newReminder.Type == reminder.Type) &&
                !updateReminders.Any(updateReminder => updateReminder.OldReminder.Type == reminder.Type));
            await RemoveEventRemindersAsync(
                remindersToDelete,
                eventArgsDto.UserId,
                reminder => _eventValidationService.CheckIfEventReminderCanBeRemoved(eventArgsDto, reminder, eventArgsDto.Recurrence));
        }

        private async Task RemoveEventRemindersAsync(
            IEnumerable<EventReminder> remindersToDelete,
            string userId,
            Action<EventReminder> validateReminderBeforeRemoval = null)
        {
            if (!remindersToDelete.Any())
            {
                return;
            }

            var timestamp = _systemClock.UtcNow;
            foreach (var reminder in remindersToDelete)
            {
                validateReminderBeforeRemoval?.Invoke(reminder);
                reminder.Modified = timestamp;
                reminder.ModifiedBy = userId;
            }
            await _uow.SaveChangesAsync(false);
            _eventRemindersDbSet.RemoveRange(remindersToDelete);
        }

        private void AddEventReminders(
            Event eventToUpdate,
            IEventArgsDto eventArgsDto,
            IEnumerable<EventReminderDto> newReminders,
            string userId)
        {
            var timestamp = _systemClock.UtcNow;
            foreach (var newReminder in newReminders)
            {
                _eventValidationService.CheckIfEventReminderCanBeAdded(eventArgsDto, newReminder);
                eventToUpdate.Reminders.Add(new EventReminder
                {
                    RemindBeforeInDays = newReminder.RemindBeforeInDays,
                    Type = newReminder.Type,
                    IsReminded = false,
                    Created = timestamp,
                    CreatedBy = userId,
                    Modified = timestamp,
                    ModifiedBy = userId
                });
            }
        }

        private void UpdateEventReminder(
            EventReminderDto reminder,
            EventReminder reminderToUpdate,
            IEventArgsDto eventArgsDto,
            Event @event)
        {
            switch (reminder.Type)
            {
                case EventReminderType.Start:
                    UpdateEventReminderStart(reminderToUpdate, eventArgsDto, @event);
                    break;

                case EventReminderType.Deadline:
                    UpdateEventReminderDeadline(reminderToUpdate, eventArgsDto, @event);
                    break;
            }

            if (reminderToUpdate.RemindBeforeInDays != reminder.RemindBeforeInDays)
            {
                _eventValidationService.CheckIfEventReminderCanBeUpdated(eventArgsDto, reminderToUpdate);
                reminderToUpdate.RemindBeforeInDays = reminder.RemindBeforeInDays;
            }

            reminderToUpdate.Modified = _systemClock.UtcNow;
            reminderToUpdate.ModifiedBy = eventArgsDto.UserId;
        }

        private static void UpdateEventReminderStart(EventReminder reminderToUpdate, IEventArgsDto eventArgsDto, Event @event)
        {
            if (eventArgsDto.StartDate != @event.StartDate)
            {
                reminderToUpdate.IsReminded = false;
            }
        }

        private static void UpdateEventReminderDeadline(EventReminder reminderToUpdate, IEventArgsDto eventArgsDto, Event @event)
        {
            if (eventArgsDto.RegistrationDeadlineDate < @event.RegistrationDeadline)
            {
                reminderToUpdate.IsReminded = false;
            }
            else if (IsRegistrationDeadlineBeingRemoved(eventArgsDto, @event))
            {
                reminderToUpdate.IsReminded = true;
            }
        }

        private static bool IsRegistrationDeadlineBeingRemoved(IEventArgsDto eventArgsDto, Event eventToUpdate)
        {
            return eventArgsDto.RegistrationDeadlineDate != eventToUpdate.RegistrationDeadline && eventArgsDto.RegistrationDeadlineDate == eventToUpdate.StartDate;
        }

        private static Expression<Func<Event, EventDetailsDto>> MapToEventDetailsDto(Guid eventId, string userId)
        {
            return e => new EventDetailsDto
            {
                Id = e.Id,
                Description = e.Description,
                ImageName = e.ImageName,
                Name = e.Name,
                Offices = new EventOfficesDto { Value = e.Offices },
                IsPinned = e.IsPinned,
                Location = e.Place,
                RegistrationDeadlineDate = e.RegistrationDeadline,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                AllowMaybeGoing = e.AllowMaybeGoing,
                AllowNotGoing = e.AllowNotGoing,
                MaxParticipants = e.MaxParticipants,
                MaxVirtualParticipants = e.MaxVirtualParticipants,
                MaxOptions = e.MaxChoices,
                HostUserId = e.ResponsibleUserId,
                WallId = e.WallId,
                HostUserFullName = e.ResponsibleUser.FirstName + " " + e.ResponsibleUser.LastName,
                // Legacy flat options only; question-owned options reach the client under Questions.
                Options = e.EventOptions
                    .Where(o => o.QuestionId == null)
                    .Select(o => new EventDetailsOptionDto
                    {
                        Id = o.Id,
                        Name = o.Option,
                        Participants = o.EventParticipants
                            .Where(x => x.EventId == eventId &&
                                  (x.AttendStatus == (int)AttendingStatus.Attending ||
                                   x.AttendStatus == (int)AttendingStatus.AttendingVirtually))
                            .Select(p => new EventDetailsParticipantDto
                            {
                                Id = p.Id,
                                UserId = p.ApplicationUser == null ? string.Empty : p.ApplicationUserId,
                                FullName = p.ApplicationUser.FirstName + " " + p.ApplicationUser.LastName,
                                ImageName = p.ApplicationUser.PictureId,
                                AttendStatus = p.AttendStatus,
                                AttendComment = p.AttendComment
                            })
                    }),
                // The only read endpoint carrying participants: /Events/Options feeds a client
                // component, so its copy of this tree must stay participant-free.
                Questions = e.EventQuestions
                    .OrderBy(question => question.Order)
                    .Select(question => new EventDetailsQuestionDto
                    {
                        Id = question.Id,
                        Title = question.Title,
                        Order = question.Order,
                        SelectType = question.SelectType,
                        IsRequired = question.IsRequired,
                        ShowIfOptionId = question.ShowIfOptionId,
                        Options = question.Options
                            .OrderBy(option => option.Order)
                            .Select(option => new EventDetailsQuestionOptionDto
                            {
                                Id = option.Id,
                                Name = option.Option,
                                Order = option.Order,
                                Participants = option.EventParticipants
                                    .Where(x => x.EventId == eventId &&
                                          (x.AttendStatus == (int)AttendingStatus.Attending ||
                                           x.AttendStatus == (int)AttendingStatus.AttendingVirtually))
                                    .Select(p => new EventDetailsParticipantDto
                                    {
                                        Id = p.Id,
                                        UserId = p.ApplicationUser == null ? string.Empty : p.ApplicationUserId,
                                        FullName = p.ApplicationUser.FirstName + " " + p.ApplicationUser.LastName,
                                        ImageName = p.ApplicationUser.PictureId,
                                        AttendStatus = p.AttendStatus,
                                        AttendComment = p.AttendComment
                                    })
                            })
                    }),
                Participants = e.EventParticipants.Select(p => new EventDetailsParticipantDto
                {
                    Id = p.Id,
                    UserId = p.ApplicationUser == null ? string.Empty : p.ApplicationUserId,
                    FullName = p.ApplicationUser.FirstName + " " + p.ApplicationUser.LastName,
                    ImageName = p.ApplicationUser.PictureId,
                    AttendStatus = p.AttendStatus,
                    AttendComment = p.AttendComment
                }),

                // Not derivable from the option-level Participants lists above: those are scoped to
                // people who are Going, so a Maybe participant would see none of their own answers.
                MyChosenOptions = e.EventParticipants
                    .Where(p => p.ApplicationUserId == userId)
                    .SelectMany(p => p.EventOptions.Select(option => option.Id))
            };
        }
    }
}
