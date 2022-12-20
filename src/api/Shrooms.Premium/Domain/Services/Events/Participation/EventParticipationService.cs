using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.Services.Wall;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;
using Shrooms.Premium.Domain.Services.Email.Event;
using Shrooms.Premium.Domain.Services.Events.Calendar;
using ISystemClock = Shrooms.Contracts.Infrastructure.ISystemClock;

namespace Shrooms.Premium.Domain.Services.Events.Participation
{
    public class EventParticipationService : IEventParticipationService
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private const string WeekOfYear = "wk";

        private readonly IUnitOfWork2 _uow;

        private readonly IDbSet<Event> _eventsDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly DbSet<EventParticipant> _eventParticipantsDbSet;

        private readonly IRoleService _roleService;
        private readonly ISystemClock _systemClock;
        private readonly IPermissionService _permissionService;
        private readonly IEventValidationService _eventValidationService;
        private readonly IWallService _wallService;
        private readonly IAsyncRunner _asyncRunner;

        public EventParticipationService(IUnitOfWork2 uow,
            ISystemClock systemClock,
            IRoleService roleService,
            IPermissionService permissionService,
            IEventValidationService eventValidationService,
            IWallService wallService,
            IAsyncRunner asyncRunner)
        {
            _uow = uow;
            _eventsDbSet = _uow.GetDbSet<Event>();
            _usersDbSet = _uow.GetDbSet<ApplicationUser>();
            _eventParticipantsDbSet = _uow.GetDbSet<EventParticipant>();

            _systemClock = systemClock;
            _permissionService = permissionService;
            _eventValidationService = eventValidationService;
            _roleService = roleService;
            _wallService = wallService;
            _asyncRunner = asyncRunner;
        }

        public async Task ResetVirtualAttendeesAsync(Guid eventId, UserAndOrganizationDto userOrg) =>
            await ResetAttendeesAsync(eventId, userOrg, AttendingStatus.AttendingVirtually);

        public async Task ResetAttendeesAsync(Guid eventId, UserAndOrganizationDto userOrg) =>
            await ResetAttendeesAsync(eventId, userOrg, AttendingStatus.Attending);

        public async Task ResetAllAttendeesAsync(Guid eventId, UserAndOrganizationDto userOrg) =>
            await ResetAttendeesAsync(eventId, userOrg, null);

        public async Task AddColleagueAsync(EventJoinDto joinDto)
        {
            await JoinAsync(joinDto, true);
        }

        public async Task JoinAsync(EventJoinDto joinDto, bool addedByColleague = false)
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                var eventDto = await GetJoinableEventAsync(joinDto);

                if (addedByColleague)
                {
                    await ValidateJoinAddPermissionsAsync(joinDto, eventDto);
                }

                eventDto.SelectedOptions = GetSelectedOptions(eventDto, joinDto);

                ValidateEventBeforeJoin(joinDto, eventDto);
                await AddAllParticipantsAsync(joinDto, eventDto);
                SendEventInvitations(joinDto, eventDto);

                if (eventDto.SendEmailToManager)
                {
                    await NotifyManagersAfterJoinAsync(joinDto, eventDto);
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task UpdateAttendStatusAsync(UpdateAttendStatusDto updateAttendStatusDto)
        {
            var @event = await _eventsDbSet
                .Include(x => x.EventParticipants)
                .Include(x => x.EventOptions)
                .Include(x => x.EventType)
                .Where(x => x.Id == updateAttendStatusDto.EventId
                            && x.OrganizationId == updateAttendStatusDto.OrganizationId)
                .Select(MapEventToJoinValidationDto)
                .FirstOrDefaultAsync();

            _eventValidationService.CheckIfEventExists(@event);
            _eventValidationService.CheckIfRegistrationDeadlineIsExpired(@event.RegistrationDeadline);
            _eventValidationService.CheckIfAttendStatusIsValid(updateAttendStatusDto.AttendStatus);
            _eventValidationService.CheckIfAttendOptionIsAllowed(updateAttendStatusDto.AttendStatus, @event);

            await AddParticipantWithStatusAsync(updateAttendStatusDto.UserId, updateAttendStatusDto.AttendStatus, updateAttendStatusDto.AttendComment, @event);

            await _uow.SaveChangesAsync(false);
        }

        public async Task DeleteByEventAsync(Guid eventId, string userId)
        {
            var participants = await _eventParticipantsDbSet
                .Where(p => p.EventId == eventId)
                .ToListAsync();

            var timestamp = DateTime.UtcNow;
            foreach (var participant in participants)
            {
                participant.Modified = timestamp;
                participant.ModifiedBy = userId;
            }

            await _uow.SaveChangesAsync(false);

            participants.ForEach(p => _eventParticipantsDbSet.Remove(p));
            await _uow.SaveChangesAsync(false);
        }

        public async Task<IEnumerable<string>> GetParticipantsEmailsIncludingHostAsync(Guid eventId)
        {
            var emailsObj = await _eventsDbSet
                .Include(e => e.EventParticipants)
                .Where(e => e.Id == eventId)
                .Select(e => new
                {
                    Participants = e.EventParticipants.Select(p => p.ApplicationUser.Email),
                    HostEmail = e.ResponsibleUser.Email
                })
                .SingleAsync();

            var result = emailsObj.Participants.ToList();
            if (!result.Contains(emailsObj.HostEmail))
            {
                result.Add(emailsObj.HostEmail);
            }

            return result;
        }

        public async Task<IEnumerable<EventUserSearchResultDto>> SearchForEventJoinAutocompleteAsync(Guid eventId, string searchString, UserAndOrganizationDto userOrg)
        {
            var searchStringLowerCase = searchString.ToLowerInvariant();
            var participants = await _eventsDbSet
                .Include(e => e.EventParticipants)
                .Where(e =>
                    e.Id == eventId &&
                    e.OrganizationId == userOrg.OrganizationId)
                .SelectMany(e => e.EventParticipants.Select(p => p.ApplicationUserId))
                .ToListAsync();

            var newUserRole = await _roleService.GetRoleIdByNameAsync(Roles.NewUser);

#pragma warning disable S1449
            var users = await _usersDbSet
                .Where(u =>
                    u.UserName.ToLower().StartsWith(searchStringLowerCase) ||
                    u.LastName.ToLower().StartsWith(searchStringLowerCase) ||
                    (u.FirstName + " " + u.LastName).ToLower().StartsWith(searchStringLowerCase))
                .Where(u =>
                    !participants.Contains(u.Id) &&
                    u.OrganizationId == userOrg.OrganizationId &&
                    u.Id != userOrg.UserId)
                .Where(_roleService.ExcludeUsersWithRole(newUserRole))
                .OrderBy(u => u.Id)
                .Select(u => new EventUserSearchResultDto
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName
                })
                .ToListAsync();
#pragma warning restore S1449

            return users;
        }

        public async Task ExpelAsync(Guid eventId, UserAndOrganizationDto userOrg, string userId)
        {
            var participant = await GetParticipantAsync(eventId, userOrg.OrganizationId, userId);
            var @event = participant.Event;

            var isAdmin = await _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Event);

            _eventValidationService.CheckIfUserHasPermission(userOrg.UserId, @event.ResponsibleUserId, isAdmin);
            _eventValidationService.CheckIfEventEndDateIsExpired(@event.EndDate);

            if (!@event.EventType.SendEmailToManager)
            {
                await RemoveParticipantAsync(participant, @event, userOrg);

                _asyncRunner.Run<IEventNotificationService>(async notifier => await notifier.NotifyRemovedEventParticipantsAsync(@event.Name, @event.Id, userOrg.OrganizationId, new[] { userId }),
                    _uow.ConnectionName);

                return;
            }

            var userEventAttendStatusDto = MapToUserEventAttendStatusChangeEmailDto(participant, @event);

            await RemoveParticipantAsync(participant, @event, userOrg);

            _asyncRunner.Run<IEventNotificationService>(async notifier => await notifier.NotifyRemovedEventParticipantsAsync(@event.Name, @event.Id, userOrg.OrganizationId, new[] { userId }),
                _uow.ConnectionName);

            await NotifyManagerAsync(userEventAttendStatusDto);
        }

        public async Task LeaveAsync(Guid eventId, UserAndOrganizationDto userOrg, string leaveComment)
        {
            var participant = await GetParticipantAsync(eventId, userOrg.OrganizationId, userOrg.UserId);
            var @event = participant.Event;

            _eventValidationService.CheckIfRegistrationDeadlineIsExpired(participant.Event.RegistrationDeadline);

            if (!participant.Event.EventType.SendEmailToManager)
            {
                await RemoveParticipantAsync(participant, @event, userOrg);
                return;
            }

            var userEventAttendStatusDto = MapToUserEventAttendStatusChangeEmailDto(participant, @event);
            await RemoveParticipantAsync(participant, @event, userOrg);
            await NotifyManagerAsync(userEventAttendStatusDto);
        }

        public async Task<IEnumerable<EventParticipantDto>> GetEventParticipantsAsync(Guid eventId, UserAndOrganizationDto userAndOrg)
        {
            var eventParticipants = (await _eventsDbSet
                .Include(e => e.EventParticipants.Select(x => x.ApplicationUser))
                .Where(e => e.Id == eventId
                            && e.OrganizationId == userAndOrg.OrganizationId
                            && e.EventParticipants.Any(p => p.AttendStatus == (int)AttendingStatus.Attending))
                .Select(MapEventToParticipantDto())
                .SingleOrDefaultAsync())?.ToList();

            _eventValidationService.CheckIfEventHasParticipants(eventParticipants);
            _eventValidationService.CheckIfEventExists(eventParticipants);

            return eventParticipants;
        }

        public async Task<int> GetMaxParticipantsCountAsync(UserAndOrganizationDto userAndOrganizationDto)
        {
            var newUserRole = await _roleService.GetRoleIdByNameAsync(Roles.NewUser);

            var maxParticipantsCount = await _usersDbSet
                .Where(_roleService.ExcludeUsersWithRole(newUserRole))
                .CountAsync(x => x.OrganizationId == userAndOrganizationDto.OrganizationId);

            return maxParticipantsCount;
        }

        public async Task UpdateSelectedOptionsAsync(EventChangeOptionsDto changeOptionsDto)
        {
            var eventEntity = await _eventsDbSet
                .Include(x => x.EventOptions)
                .Include(x => x.EventParticipants)
                .Where(x => x.Id == changeOptionsDto.EventId && x.OrganizationId == changeOptionsDto.OrganizationId)
                .Select(MapEventToJoinValidationDto)
                .FirstOrDefaultAsync();

            _eventValidationService.CheckIfEventExists(eventEntity);

            // ReSharper disable once PossibleNullReferenceException
            eventEntity.SelectedOptions = eventEntity.Options
                .Where(option => changeOptionsDto.ChosenOptions.Contains(option.Id))
                .ToList();

            _eventValidationService.CheckIfRegistrationDeadlineIsExpired(eventEntity.RegistrationDeadline);
            _eventValidationService.CheckIfProvidedOptionsAreValid(changeOptionsDto.ChosenOptions, eventEntity.SelectedOptions);
            _eventValidationService.CheckIfJoiningNotEnoughChoicesProvided(eventEntity.MaxChoices, changeOptionsDto.ChosenOptions.Count());
            _eventValidationService.CheckIfJoiningTooManyChoicesProvided(eventEntity.MaxChoices, changeOptionsDto.ChosenOptions.Count());
            _eventValidationService.CheckIfSingleChoiceSelectedWithRule(eventEntity.SelectedOptions, OptionRules.IgnoreSingleJoin);
            _eventValidationService.CheckIfUserParticipatesInEvent(changeOptionsDto.UserId, eventEntity.Participants);

            await ValidateSingleJoinForSameTypeEventsIfNeededAsync(eventEntity, changeOptionsDto.OrganizationId, changeOptionsDto.UserId);

            var participant = await _eventParticipantsDbSet
                .Include(x => x.EventOptions)
                .FirstAsync(p => p.EventId == changeOptionsDto.EventId && p.ApplicationUserId == changeOptionsDto.UserId);

            participant.EventOptions.Clear();
            participant.EventOptions = eventEntity.SelectedOptions;

            await _uow.SaveChangesAsync(changeOptionsDto.UserId);
        }

        private void ValidateEventBeforeJoin(EventJoinDto joinDto, EventJoinValidationDto eventDto)
        {
            _eventValidationService.CheckIfRegistrationDeadlineIsExpired(eventDto.RegistrationDeadline);
            _eventValidationService.CheckIfProvidedOptionsAreValid(joinDto.ChosenOptions, eventDto.SelectedOptions);
            _eventValidationService.CheckIfJoiningNotEnoughChoicesProvided(eventDto.MaxChoices, joinDto.ChosenOptions.Count());
            _eventValidationService.CheckIfJoiningTooManyChoicesProvided(eventDto.MaxChoices, joinDto.ChosenOptions.Count());
            _eventValidationService.CheckIfSingleChoiceSelectedWithRule(eventDto.SelectedOptions, OptionRules.IgnoreSingleJoin);
            _eventValidationService.CheckIfJoinAttendStatusIsValid(joinDto.AttendStatus, eventDto);
            _eventValidationService.CheckIfCanJoinEvent(joinDto, eventDto);
        }

        private void NotifyManagers(IEnumerable<UserEventAttendStatusChangeEmailDto> userEventAttendStatusChangeEmailDtos)
        {
            foreach (var user in userEventAttendStatusChangeEmailDtos)
            {
                if (user.ManagerEmail == null)
                {
                    continue;
                }

                _asyncRunner.Run<IEventNotificationService>(async notifier => await notifier.NotifyManagerAboutEventAsync(user, false),
                    _uow.ConnectionName);
            }
        }

        private EventParticipantAttendDto GetAttendingParticipant(EventJoinValidationDto validationDto, string userId) =>
            validationDto.Participants.SingleOrDefault(p => p.Id == userId);

        private async Task ValidateParticipantIsValidUser(EventJoinDto joinDto, string userId)
        {
            var userExists = await _usersDbSet.AnyAsync(x => x.Id == userId && x.OrganizationId == joinDto.OrganizationId);
            _eventValidationService.CheckIfUserExists(userExists);
        }

        private async Task ResetAttendeesAsync(Guid eventId, UserAndOrganizationDto userOrg, AttendingStatus? status)
        {
            var @event = await _eventsDbSet
              .Include(e => e.EventParticipants)
              .Include(e => e.EventOptions)
              .Include(e => e.EventType)
              .Include(e => e.EventParticipants.Select(participant => participant.ApplicationUser))
              .Include(e => e.EventParticipants.Select(participant => participant.ApplicationUser.Manager))
              .SingleOrDefaultAsync(e => e.Id == eventId && e.OrganizationId == userOrg.OrganizationId);

            if (!@event.EventParticipants.Any())
            {
                return;
            }

            _eventValidationService.CheckIfEventExists(@event);
            var hasPermission = await _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Event);

            // ReSharper disable once PossibleNullReferenceException
            _eventValidationService.CheckIfUserHasPermission(userOrg.UserId, @event.ResponsibleUserId, hasPermission);
            _eventValidationService.CheckIfEventEndDateIsExpired(@event.EndDate);

            var timestamp = DateTime.UtcNow;
            foreach (var participant in @event.EventParticipants)
            {
                participant.UpdateMetadata(userOrg.UserId, timestamp);
            }

            await _uow.SaveChangesAsync(false);

            if (!@event.EventType.SendEmailToManager)
            {
                await RemoveParticipantsAsync(@event, userOrg, status);
                return;
            }

            var userEventAttendStatusDto = MapEventToUserEventAttendStatusChangeEmailDto(@event).ToList();
            await RemoveParticipantsAsync(@event, userOrg, status);
            NotifyManagers(userEventAttendStatusDto);
        }

        private static Func<EventParticipant, bool> FilterParticipantByAttendStatus(AttendingStatus? status) =>
            participant => status == null || participant.AttendStatus == (int)status.Value;

        private async Task NotifyManagersAfterJoinAsync(EventJoinDto joinDto, EventJoinValidationDto eventJoinValidationDto)
        {
            var users = await _usersDbSet
                .Where(user => joinDto.ParticipantIds.Contains(user.Id) && joinDto.OrganizationId == user.OrganizationId)
                .ToListAsync();

            _eventValidationService.CheckIfAllRequestParticipantsHaveAccounts(users, joinDto.ParticipantIds);

            var managerIds = users.Select(user => user.ManagerId).ToHashSet();

            var managers = await _usersDbSet
                .Where(manager => managerIds.Contains(manager.Id))
                .ToDictionaryAsync(manager => manager.Id, manager => manager.Email);

            if (!managers.Any())
            {
                return;
            }

            var attendees = users.Join(eventJoinValidationDto.Participants,
                user => user.Id,
                participant => participant.Id,
                (user, participant) => new
                {
                    User = user,
                    AttendStatus = participant.AttendStatus
                });

            foreach (var attendee in attendees)
            {
                if (IsUserSwitchingJoinStatus(joinDto, attendee.AttendStatus) ||
                    !TryGetManagerEmail(attendee.User, managers, out var managerEmail))
                {
                    continue;
                }

                var userAttendStatusDto = MapToUserEventAttendStatusChangeEmailDto(attendee.User, eventJoinValidationDto, managerEmail);

                _asyncRunner.Run<IEventNotificationService>(
                    async notifier => await notifier.NotifyManagerAboutEventAsync(userAttendStatusDto, true),
                    _uow.ConnectionName);
            }
        }

        private static bool IsUserSwitchingJoinStatus(EventJoinDto joinDto, int attendStatus) =>
            joinDto.AttendStatus != attendStatus;

        private bool TryGetManagerEmail(ApplicationUser user, Dictionary<string, string> managers, out string managerEmail)
        {
            if (user.ManagerId == null)
            {
                managerEmail = null;
                return false;
            }

            return managers.TryGetValue(user.ManagerId, out managerEmail);
        }

        private async Task NotifyManagerAsync(UserEventAttendStatusChangeEmailDto userAttendStatusDto)
        {
            var managerEmail = await _usersDbSet
                .Where(user => user.Id == userAttendStatusDto.ManagerId)
                .Select(user => user.Email)
                .FirstOrDefaultAsync();

            if (managerEmail == null)
            {
                return;
            }

            userAttendStatusDto.ManagerEmail = managerEmail;

            _asyncRunner.Run<IEventNotificationService>(
                async notifier => await notifier.NotifyManagerAboutEventAsync(userAttendStatusDto, false),
                _uow.ConnectionName);
        }

        private async Task JoinOrLeaveEventWallAsync(string responsibleUserId, string wallParticipantId, int wallId, UserAndOrganizationDto userOrg)
        {
            if (responsibleUserId == wallParticipantId)
            {
                return;
            }

            await _wallService.JoinOrLeaveWallAsync(wallId, wallParticipantId, wallParticipantId, userOrg.OrganizationId, true);
        }

        private async Task RemoveParticipantAsync(EventParticipant participant, Event @event, UserAndOrganizationDto userOrg)
        {
            var timestamp = DateTime.UtcNow;

            participant.UpdateMetadata(userOrg.UserId, timestamp);

            await _uow.SaveChangesAsync(false);

            await JoinOrLeaveEventWallAsync(@event.ResponsibleUserId, participant.ApplicationUserId, @event.WallId, userOrg);

            _eventParticipantsDbSet.Remove(participant);

            await _uow.SaveChangesAsync(false);
        }

        private async Task RemoveParticipantsAsync(Event @event, UserAndOrganizationDto userOrg, AttendingStatus? status)
        {
            var filteredParticipants = @event.EventParticipants.Where(FilterParticipantByAttendStatus(status));
            var filteredParticipantIds = filteredParticipants.Select(participant => participant.ApplicationUserId);

            await RemoveParticipantsFromEventWallAsync(@event, userOrg, filteredParticipants);
            _eventParticipantsDbSet.RemoveRange(filteredParticipants);
            await _uow.SaveChangesAsync(false);

            NotifyRemovedParticipants(@event, userOrg, filteredParticipantIds);
        }

        private async Task RemoveParticipantsFromEventWallAsync(Event @event, UserAndOrganizationDto userOrg, IEnumerable<EventParticipant> filteredParticipants)
        {
            foreach (var participant in filteredParticipants)
            {
                await JoinOrLeaveEventWallAsync(@event.ResponsibleUserId, participant.ApplicationUserId, @event.WallId, userOrg);
            }
        }

        private void NotifyRemovedParticipants(Event @event, UserAndOrganizationDto userOrg, IEnumerable<string> filteredParticipantIds)
        {
            _asyncRunner.Run<IEventNotificationService>(async notifier =>
                            await notifier.NotifyRemovedEventParticipantsAsync(@event.Name, @event.Id, userOrg.OrganizationId, filteredParticipantIds),
                            _uow.ConnectionName);
        }

        private async Task<EventParticipant> GetParticipantAsync(Guid eventId, int userOrg, string userId)
        {
            var participant = await _eventParticipantsDbSet
                .Include(p => p.Event)
                .Include(p => p.Event.EventType)
                .Include(p => p.EventOptions)
                .Include(p => p.ApplicationUser)
                .SingleOrDefaultAsync(p => p.EventId == eventId &&
                                           p.Event.OrganizationId == userOrg &&
                                           p.ApplicationUserId == userId);

            _eventValidationService.CheckIfEventExists(participant);
            _eventValidationService.CheckIfParticipantExists(participant);

            return participant;
        }

        private static Expression<Func<Event, IEnumerable<EventParticipantDto>>> MapEventToParticipantDto()
        {
            return e => e.EventParticipants.Select(p => new EventParticipantDto
            {
                FirstName = string.IsNullOrEmpty(p.ApplicationUser.FirstName)
                    ? BusinessLayerConstants.DeletedUserFirstName
                    : p.ApplicationUser.FirstName,

                LastName = string.IsNullOrEmpty(p.ApplicationUser.LastName)
                    ? BusinessLayerConstants.DeletedUserLastName
                    : p.ApplicationUser.LastName
            });
        }

        private static UserEventAttendStatusChangeEmailDto MapToUserEventAttendStatusChangeEmailDto(ApplicationUser user, EventJoinValidationDto eventJoinValidationDto, string managerEmail)
        {
            return new UserEventAttendStatusChangeEmailDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                ManagerEmail = managerEmail,
                ManagerId = user.ManagerId,
                EventName = eventJoinValidationDto.Name,
                EventId = eventJoinValidationDto.Id,
                OrganizationId = user.OrganizationId,
                EventStartDate = eventJoinValidationDto.StartDate,
                EventEndDate = eventJoinValidationDto.EndDate
            };
        }

        private static UserEventAttendStatusChangeEmailDto MapToUserEventAttendStatusChangeEmailDto(EventParticipant participant, Event @event)
        {
            return new UserEventAttendStatusChangeEmailDto
            {
                FirstName = participant.ApplicationUser.FirstName,
                LastName = participant.ApplicationUser.LastName,
                OrganizationId = participant.ApplicationUser.OrganizationId,
                EventName = @event.Name,
                EventId = @event.Id,
                EventStartDate = @event.StartDate,
                EventEndDate = @event.EndDate,
                ManagerId = participant.ApplicationUser.ManagerId
            };
        }

        private static IEnumerable<UserEventAttendStatusChangeEmailDto> MapEventToUserEventAttendStatusChangeEmailDto(Event @event)
        {
            return @event.EventParticipants.Select(participant => new UserEventAttendStatusChangeEmailDto
            {
                FirstName = participant.ApplicationUser.FirstName,
                LastName = participant.ApplicationUser.LastName,
                OrganizationId = participant.ApplicationUser.OrganizationId,
                EventName = @event.Name,
                EventEndDate = @event.EndDate,
                EventStartDate = @event.StartDate,
                ManagerEmail = participant.ApplicationUser.Manager?.Email,
                ManagerId = participant.ApplicationUser.ManagerId
            });
        }

        private static Expression<Func<Event, EventJoinValidationDto>> MapEventToJoinValidationDto =>
            e => new EventJoinValidationDto
            {
                Participants = e.EventParticipants
                    .Where(x => x.AttendStatus == (int)AttendingStatus.Attending ||
                                x.AttendStatus == (int)AttendingStatus.AttendingVirtually)
                    .Select(x => new EventParticipantAttendDto
                    {
                        Id = x.ApplicationUserId,
                        AttendStatus = x.AttendStatus
                    })
                    .ToList(),
                MaxParticipants = e.MaxParticipants,
                MaxVirtualParticipants = e.MaxVirtualParticipants,
                StartDate = e.StartDate,
                MaxChoices = e.MaxChoices,
                Id = e.Id,
                Options = e.EventOptions,
                IsSingleJoin = e.EventType.IsSingleJoin,
                EventTypeId = e.EventTypeId,
                SingleJoinGroupName = e.EventType.SingleJoinGroupName,
                Name = e.Name,
                EndDate = e.EndDate,
                Description = e.Description,
                AllowMaybeGoing = e.AllowMaybeGoing,
                AllowNotGoing = e.AllowNotGoing,
                Location = e.Place,
                RegistrationDeadline = e.RegistrationDeadline,
                ResponsibleUserId = e.ResponsibleUserId,
                WallId = e.WallId,
                SendEmailToManager = e.EventType.SendEmailToManager
            };

        private async Task ValidateSingleJoinForSameTypeEventsIfNeededAsync(EventJoinValidationDto validationDto, int orgId, string userId)
        {
            if (validationDto.SelectedOptions.All(x => x.Rule == OptionRules.IgnoreSingleJoin) &&
                validationDto.SelectedOptions.Count != 0 ||
                !validationDto.IsSingleJoin)
            {
                return;
            }

            var query = _eventsDbSet
                .Include(e => e.EventParticipants.Select(x => x.EventOptions))
                .Include(e => e.EventType)
                .Where(e =>
                    e.OrganizationId == orgId &&
                    e.Id != validationDto.Id &&
                    SqlFunctions.DatePart(WeekOfYear, e.StartDate) == SqlFunctions.DatePart(WeekOfYear, validationDto.StartDate) &&
                    e.StartDate.Year == validationDto.StartDate.Year &&
                    e.EventParticipants.Any(p => p.ApplicationUserId == userId &&
                                                 (p.AttendStatus == (int)AttendingStatus.Attending ||
                                                  p.AttendStatus == (int)AttendingStatus.AttendingVirtually)));

            query = string.IsNullOrEmpty(validationDto.SingleJoinGroupName)
                ? query.Where(x => x.EventType.Id == validationDto.EventTypeId)
                : query.Where(x => x.EventType.SingleJoinGroupName == validationDto.SingleJoinGroupName);

            var anyEventsAlreadyJoined = await query.AnyAsync(x => !x.EventParticipants.Any(y =>
                y.ApplicationUserId == userId &&
                y.EventOptions.All(z => z.Rule == OptionRules.IgnoreSingleJoin) &&
                y.EventOptions.Count > 0));

            _eventValidationService.CheckIfUserExistsInOtherSingleJoinEvent(anyEventsAlreadyJoined);
        }

        private async Task AddParticipantAsync(string userId, Guid eventId, ICollection<EventOption> eventOptions, int attendStatus)
        {
            var timeStamp = _systemClock.UtcNow;
            var participant = await _eventParticipantsDbSet
                .Include(x => x.EventOptions)
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.ApplicationUserId == userId);

            if (participant == null)
            {
                var newParticipant = new EventParticipant
                {
                    ApplicationUserId = userId,
                    Created = timeStamp,
                    CreatedBy = userId,
                    EventId = eventId,
                    Modified = timeStamp,
                    ModifiedBy = userId,
                    EventOptions = eventOptions,
                    AttendComment = string.Empty,
                    AttendStatus = attendStatus
                };
                _eventParticipantsDbSet.Add(newParticipant);
            }
            else
            {
                participant.Modified = timeStamp;
                participant.ModifiedBy = userId;
                participant.EventOptions = eventOptions;
                participant.AttendStatus = attendStatus;
                participant.AttendComment = string.Empty;
            }
        }

        private async Task AddParticipantWithStatusAsync(string userId, int attendingStatus, string attendComment, EventJoinValidationDto eventDto)
        {
            var timeStamp = _systemClock.UtcNow;
            var participant = await _eventParticipantsDbSet.FirstOrDefaultAsync(p => p.EventId == eventDto.Id && p.ApplicationUserId == userId);

            if (participant != null)
            {
                participant.AttendStatus = attendingStatus;
                participant.AttendComment = attendComment;
                participant.Modified = timeStamp;
                participant.ModifiedBy = userId;
            }
            else
            {
                var newParticipant = new EventParticipant
                {
                    ApplicationUserId = userId,
                    Created = timeStamp,
                    CreatedBy = userId,
                    EventId = eventDto.Id,
                    Modified = timeStamp,
                    ModifiedBy = userId,
                    AttendComment = attendComment,
                    AttendStatus = attendingStatus
                };

                _eventParticipantsDbSet.Add(newParticipant);
            }
        }

        private async Task<EventJoinValidationDto> GetJoinableEventAsync(EventJoinDto joinDto)
        {
            var eventDto = await _eventsDbSet
                .Include(x => x.EventParticipants)
                .Include(x => x.EventOptions)
                .Include(x => x.EventType)
                .Where(x => x.Id == joinDto.EventId && x.OrganizationId == joinDto.OrganizationId)
                .Select(MapEventToJoinValidationDto)
                .FirstOrDefaultAsync();
            _eventValidationService.CheckIfEventExists(eventDto);

            return eventDto;
        }

        private async Task ValidateJoinAddPermissionsAsync(EventJoinDto joinDto, EventJoinValidationDto eventDto)
        {
            var hasPermission = await _permissionService.UserHasPermissionAsync(joinDto, AdministrationPermissions.Event);
            _eventValidationService.CheckIfUserHasPermission(joinDto.UserId, eventDto.ResponsibleUserId, hasPermission);
        }

        private List<EventOption> GetSelectedOptions(EventJoinValidationDto validationDto, EventJoinDto joinDto) =>
            validationDto.Options.Where(option => joinDto.ChosenOptions.Contains(option.Id))
                .ToList();

        private void SendEventInvitations(EventJoinDto joinDto, EventJoinValidationDto eventDto) =>
            _asyncRunner.Run<IEventCalendarService>(async notifier =>
                await notifier.SendInvitationAsync(eventDto, joinDto.ParticipantIds, joinDto.OrganizationId), _uow.ConnectionName);

        private async Task AddAllParticipantsAsync(EventJoinDto joinDto, EventJoinValidationDto eventDto)
        {
            foreach (var userId in joinDto.ParticipantIds)
            {
                await ValidateParticipantIsValidUser(joinDto, userId);
                var participantDto = GetAttendingParticipant(eventDto, userId);

                if (!IsSwitchingJoinAttendStatus(joinDto, participantDto))
                {
                    _eventValidationService.CheckIfUserAlreadyJoinedSameEvent(participantDto);
                    await ValidateSingleJoinForSameTypeEventsIfNeededAsync(eventDto, joinDto.OrganizationId, userId);
                }

                await AddParticipantAsync(userId, eventDto.Id, eventDto.SelectedOptions, joinDto.AttendStatus);
                await JoinOrLeaveEventWallAsync(eventDto.ResponsibleUserId, userId, eventDto.WallId, joinDto);
            }

            await _uow.SaveChangesAsync(false);
        }

        private static bool IsSwitchingJoinAttendStatus(EventJoinDto joinDto, EventParticipantAttendDto participantDto) =>
            participantDto != null && joinDto.AttendStatus != participantDto.AttendStatus;
    }
}
