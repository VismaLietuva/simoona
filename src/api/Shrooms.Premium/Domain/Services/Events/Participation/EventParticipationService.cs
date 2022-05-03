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
        private readonly IDbSet<EventParticipant> _eventParticipantsDbSet;

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

        public async Task ResetAttendeesAsync(Guid eventId, UserAndOrganizationDto userOrg)
        {
            var @event = await _eventsDbSet
                .Include(e => e.EventParticipants)
                .Include(e => e.EventOptions)
                .SingleOrDefaultAsync(e => e.Id == eventId && e.OrganizationId == userOrg.OrganizationId);

            _eventValidationService.CheckIfEventExists(@event);
            var hasPermission = await _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Event);

            // ReSharper disable once PossibleNullReferenceException
            _eventValidationService.CheckIfUserHasPermission(userOrg.UserId, @event.ResponsibleUserId, hasPermission);
            _eventValidationService.CheckIfEventEndDateIsExpired(@event.EndDate);

            var users = @event.EventParticipants.Select(p => p.ApplicationUserId).ToList();
            var timestamp = DateTime.UtcNow;

            foreach (var participant in @event.EventParticipants)
            {
                participant.UpdateMetadata(userOrg.UserId, timestamp);
            }

            await _uow.SaveChangesAsync(false);

            foreach (var participant in @event.EventParticipants.ToList())
            {
                await JoinOrLeaveEventWallAsync(@event.ResponsibleUserId, participant.ApplicationUserId, @event.WallId, userOrg);
                _eventParticipantsDbSet.Remove(participant);
            }

            await _uow.SaveChangesAsync(false);

            _asyncRunner.Run<IEventNotificationService>(async notifier => await notifier.NotifyRemovedEventParticipantsAsync(@event.Name, @event.Id, userOrg.OrganizationId, users),
                _uow.ConnectionName);
        }

        public async Task AddColleagueAsync(EventJoinDto joinDto)
        {
            await JoinAsync(joinDto, true);
        }

        public async Task JoinAsync(EventJoinDto joinDto, bool addedByColleague = false)
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                var @event = _eventsDbSet
                    .Include(x => x.EventParticipants)
                    .Include(x => x.EventOptions)
                    .Include(x => x.EventType)
                    .Where(x => x.Id == joinDto.EventId && x.OrganizationId == joinDto.OrganizationId)
                    .Select(MapEventToJoinValidationDto)
                    .FirstOrDefault();

                _eventValidationService.CheckIfEventExists(@event);

                if (addedByColleague)
                {
                    var hasPermission = await _permissionService.UserHasPermissionAsync(joinDto, AdministrationPermissions.Event);
                    if (@event != null)
                    {
                        _eventValidationService.CheckIfUserHasPermission(joinDto.UserId, @event.ResponsibleUserId, hasPermission);
                    }
                }

                // ReSharper disable once PossibleNullReferenceException
                @event.SelectedOptions = @event.Options
                    .Where(option => joinDto.ChosenOptions.Contains(option.Id))
                    .ToList();

                _eventValidationService.CheckIfRegistrationDeadlineIsExpired(@event.RegistrationDeadline);
                _eventValidationService.CheckIfProvidedOptionsAreValid(joinDto.ChosenOptions, @event.SelectedOptions);
                _eventValidationService.CheckIfJoiningNotEnoughChoicesProvided(@event.MaxChoices, joinDto.ChosenOptions.Count());
                _eventValidationService.CheckIfJoiningTooManyChoicesProvided(@event.MaxChoices, joinDto.ChosenOptions.Count());
                _eventValidationService.CheckIfSingleChoiceSelectedWithRule(@event.SelectedOptions, OptionRules.IgnoreSingleJoin);
                _eventValidationService.CheckIfEventHasEnoughPlaces(@event.MaxParticipants, @event.Participants.Count + joinDto.ParticipantIds.Count);
                _eventValidationService.CheckIfAttendOptionIsAllowed(joinDto.AttendStatus, @event);

                foreach (var userId in joinDto.ParticipantIds)
                {
                    var userExists = await _usersDbSet.AnyAsync(x => x.Id == userId && x.OrganizationId == joinDto.OrganizationId);

                    _eventValidationService.CheckIfUserExists(userExists);

                    var alreadyParticipates = @event.Participants.Any(p => p == userId);
                    _eventValidationService.CheckIfUserAlreadyJoinedSameEvent(alreadyParticipates);

                    await ValidateSingleJoinAsync(@event, joinDto.OrganizationId, userId);
                    await AddParticipanAsync(userId, @event.Id, @event.SelectedOptions);

                    await JoinOrLeaveEventWallAsync(@event.ResponsibleUserId, userId, @event.WallId, joinDto);
                }

                await _uow.SaveChangesAsync(false);

                await NotifyManagersAsync(joinDto, @event);

                _asyncRunner.Run<IEventCalendarService>(async notifier => await notifier.SendInvitationAsync(@event, joinDto.ParticipantIds, joinDto.OrganizationId), _uow.ConnectionName);

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

            // ReSharper disable once PossibleNullReferenceException
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

        // I changed the sequence of this
        public async Task ExpelAsync(Guid eventId, UserAndOrganizationDto userOrg, string userId)
        {
            var participant = await GetParticipantAsync(eventId, userOrg.OrganizationId, userId);
            var @event = participant.Event;

            var isAdmin = await _permissionService.UserHasPermissionAsync(userOrg, AdministrationPermissions.Event);
            _eventValidationService.CheckIfUserHasPermission(userOrg.UserId, @event.ResponsibleUserId, isAdmin);
            _eventValidationService.CheckIfEventEndDateIsExpired(@event.EndDate);

            await JoinOrLeaveEventWallAsync(@event.ResponsibleUserId, userId, @event.WallId, userOrg);
            await NotifyManagerAsync(userOrg, await RemoveParticipantAsync(userId, participant));

            _asyncRunner.Run<IEventNotificationService>(async notifier => await notifier.NotifyRemovedEventParticipantsAsync(@event.Name, @event.Id, userOrg.OrganizationId, new[] { userId }),
                _uow.ConnectionName);
        }

        public async Task LeaveAsync(Guid eventId, UserAndOrganizationDto userOrg, string leaveComment)
        {
            var participant = await GetParticipantAsync(eventId, userOrg.OrganizationId, userOrg.UserId);
            _eventValidationService.CheckIfRegistrationDeadlineIsExpired(participant.Event.RegistrationDeadline);

            await JoinOrLeaveEventWallAsync(participant.Event.ResponsibleUserId, userOrg.UserId, participant.Event.WallId, userOrg);
            await NotifyManagerAsync(userOrg, await RemoveParticipantAsync(userOrg.UserId, participant));
        }

        public async Task<IEnumerable<EventParticipantDto>> GetEventParticipantsAsync(Guid eventId, UserAndOrganizationDto userAndOrg)
        {
            var eventParticipants = (await _eventsDbSet
                .Include(e => e.EventParticipants.Select(x => x.ApplicationUser))
                .Where(e => e.Id == eventId
                            && e.OrganizationId == userAndOrg.OrganizationId
                            && e.EventParticipants.Any(p => p.AttendStatus == (int)AttendingStatus.Attending)) // && p.ApplicationUserId == userAndOrg.UserId))
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

            await ValidateSingleJoinAsync(eventEntity, changeOptionsDto.OrganizationId, changeOptionsDto.UserId);

            var participant = await _eventParticipantsDbSet
                .Include(x => x.EventOptions)
                .FirstAsync(p => p.EventId == changeOptionsDto.EventId && p.ApplicationUserId == changeOptionsDto.UserId);

            participant.EventOptions.Clear();
            participant.EventOptions = eventEntity.SelectedOptions;

            await _uow.SaveChangesAsync(changeOptionsDto.UserId);
        }

        private async Task NotifyManagersAsync(EventJoinDto joinDto, EventJoinValidationDto eventJoinValidationDto)
        {
            if (!eventJoinValidationDto.SendEmailToManager)
            {
                return;
            }

            var users = await _usersDbSet
                .Where(user => joinDto.ParticipantIds
                .Contains(user.Id) && joinDto.OrganizationId == user.OrganizationId)
                .ToListAsync();

            var managerIds = users.Select(user => user.ManagerId).ToHashSet();

            var managers = await _usersDbSet
                .Where(manager => managerIds.Contains(manager.Id))
                .ToDictionaryAsync(manager => manager.Id, manager => manager.Email);
;
            foreach (var user in users)
            {
                if (!managers.TryGetValue(user.ManagerId, out var managerEmail))
                {
                    continue;
                }

                var userAttendStatusDto = new UserEventAttendStatusChangeEmailDto
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    ManagerEmail = managerEmail,
                    ManagerId = user.ManagerId,
                    EventName = eventJoinValidationDto.Name,
                    EventId = eventJoinValidationDto.Id,
                    OrganizationId = user.OrganizationId,
                    EventStartDate = eventJoinValidationDto.StartDate,
                    EventEndDate = eventJoinValidationDto.EndDate
                };

                _asyncRunner.Run<IEventNotificationService>(
                    async notifier => await notifier.NotifyManagerAboutEventAsync(userAttendStatusDto, true),
                    _uow.ConnectionName);
            }
        }

        private async Task NotifyManagerAsync(UserAndOrganizationDto userOrg, EventParticipant participant)
        {
            if (!participant.Event.EventType.SendEmailToManager)
            {
                return;
            }

            var managerEmail = await _usersDbSet
                .Where(user => user.Id == participant.ApplicationUser.ManagerId)
                .Select(user => user.Email)
                .FirstOrDefaultAsync();

            var userAttendStatusDto = new UserEventAttendStatusChangeEmailDto
            {
                FirstName = participant.ApplicationUser.FirstName,
                LastName = participant.ApplicationUser.LastName,
                Email = participant.ApplicationUser.Email,
                OrganizationId = userOrg.OrganizationId,
                ManagerEmail = managerEmail,
                EventId = participant.EventId,
                EventName = participant.Event.Name
            };

            _asyncRunner.Run<IEventNotificationService>(
                async notifier => await notifier.NotifyManagerAboutEventAsync(userAttendStatusDto, false),
                _uow.ConnectionName);
        }

        private async Task JoinOrLeaveEventWallAsync(string responsibleUserId, string wallParticipantId, int wallId, UserAndOrganizationDto userOrg)
        {
            if (responsibleUserId != wallParticipantId)
            {
                await _wallService.JoinOrLeaveWallAsync(wallId, wallParticipantId, wallParticipantId, userOrg.OrganizationId, true);
            }
        }

        private async Task<EventParticipant> RemoveParticipantAsync(string userId, EventParticipant participant)
        {
            var timestamp = DateTime.UtcNow;

            participant.UpdateMetadata(userId, timestamp);

            await _uow.SaveChangesAsync(false);

            var removedParticipant = new EventParticipant
            {
                ApplicationUser = participant.ApplicationUser,
                Event = participant.Event,
                EventId = participant.EventId,
            };

            _eventParticipantsDbSet.Remove(participant);

            await _uow.SaveChangesAsync(false);

            return removedParticipant;
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

        private static Expression<Func<Event, EventJoinValidationDto>> MapEventToJoinValidationDto =>
            e => new EventJoinValidationDto
            {
                Participants = e.EventParticipants
                    .Where(x => x.AttendStatus == (int)AttendingStatus.Attending)
                    .Select(x => x.ApplicationUserId)
                    .ToList(),
                MaxParticipants = e.MaxParticipants,
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

        private async Task ValidateSingleJoinAsync(EventJoinValidationDto validationDto, int orgId, string userId)
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
                                                 p.AttendStatus == (int)AttendingStatus.Attending));

            query = string.IsNullOrEmpty(validationDto.SingleJoinGroupName)
                ? query.Where(x => x.EventType.Id == validationDto.EventTypeId)
                : query.Where(x => x.EventType.SingleJoinGroupName == validationDto.SingleJoinGroupName);

            var anyEventsAlreadyJoined = await query.AnyAsync(x => !x.EventParticipants.Any(y =>
                y.ApplicationUserId == userId &&
                y.EventOptions.All(z => z.Rule == OptionRules.IgnoreSingleJoin) &&
                y.EventOptions.Count > 0));

            _eventValidationService.CheckIfUserExistsInOtherSingleJoinEvent(anyEventsAlreadyJoined);
        }

        private async Task AddParticipanAsync(string userId, Guid eventId, ICollection<EventOption> eventOptions)
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
                    AttendStatus = (int)AttendingStatus.Attending
                };
                _eventParticipantsDbSet.Add(newParticipant);
            }
            else
            {
                participant.Modified = timeStamp;
                participant.ModifiedBy = userId;
                participant.EventOptions = eventOptions;
                participant.AttendStatus = (int)AttendingStatus.Attending;
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
    }
}
