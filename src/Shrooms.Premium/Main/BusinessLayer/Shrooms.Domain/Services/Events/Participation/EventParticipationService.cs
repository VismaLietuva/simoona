﻿using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.BusinessLayer;
using Shrooms.Constants.BusinessLayer.Events;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Email.Event;
using Shrooms.Domain.Services.Events.Calendar;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.Services.Wall;
using Shrooms.DomainServiceValidators.Validators.Events;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Events;
using Shrooms.Infrastructure.FireAndForget;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using ISystemClock = Shrooms.Infrastructure.SystemClock.ISystemClock;

namespace Shrooms.Domain.Services.Events.Participation
{
    public class EventParticipationService : IEventParticipationService
    {
        private static object multiUserJoinLock = new object();

        private readonly IUnitOfWork2 _uow;

        private readonly IDbSet<Event> _eventsDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<EventParticipant> _eventParticipantsDbSet;

        private readonly IRoleService _roleService;
        private readonly ISystemClock _systemClock;
        private readonly IPermissionService _permissionService;
        private readonly IEventCalendarService _calendarService;
        private readonly IEventValidationService _eventValidationService;
        private readonly IWallService _wallService;
        private readonly IAsyncRunner _asyncRunner;

        public EventParticipationService(
            IUnitOfWork2 uow,
            ISystemClock systemClock,
            IRoleService roleService,
            IPermissionService permissionService,
            IEventCalendarService calendarService,
            IEventValidationService eventValidationService,
            IWallService wallService,
            IAsyncRunner asyncRunner)
        {
            _uow = uow;
            _eventsDbSet = _uow.GetDbSet<Event>();
            _usersDbSet = _uow.GetDbSet<ApplicationUser>();
            _eventParticipantsDbSet = _uow.GetDbSet<EventParticipant>();

            _systemClock = systemClock;
            _calendarService = calendarService;
            _permissionService = permissionService;
            _eventValidationService = eventValidationService;
            _roleService = roleService;
            _wallService = wallService;
            _asyncRunner = asyncRunner;
        }

        public void ResetAttendees(Guid eventId, UserAndOrganizationDTO userOrg)
        {
            var @event = _eventsDbSet
                .Include(e => e.EventParticipants)
                .Include(e => e.EventOptions)
                .SingleOrDefault(e =>
                    e.Id == eventId &&
                    e.OrganizationId == userOrg.OrganizationId);

            _eventValidationService.CheckIfEventExists(@event);
            var hasPermission = _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Event);
            _eventValidationService.CheckIfUserHasPermission(userOrg.UserId, @event.ResponsibleUserId, hasPermission);
            _eventValidationService.CheckIfEventEndDateIsExpired(@event.EndDate);

            var users = @event.EventParticipants.Select(p => p.ApplicationUserId).ToList();
            var timestamp = DateTime.UtcNow;

            foreach (var participant in @event.EventParticipants)
            {
                participant.UpdateMetadata(userOrg.UserId, timestamp);
            }

            _uow.SaveChanges(false);

            foreach (var participant in @event.EventParticipants.ToList())
            {
                JoinLeaveEventWall(@event.ResponsibleUserId, participant.ApplicationUserId, @event.WallId, userOrg);
                _eventParticipantsDbSet.Remove(participant);
            }

            _uow.SaveChanges(false);

            _asyncRunner.Run<IEventNotificationService>(n => n.NotifyRemovedEventParticipants(@event.Name, @event.Id, userOrg.OrganizationId, users), _uow.ConnectionName);
            _asyncRunner.Run<IEventCalendarService>(n => n.ResetParticipants(@event.Id, userOrg.OrganizationId), _uow.ConnectionName);
        }

        public void Join(EventJoinDTO joinDto)
        {
            lock (multiUserJoinLock)
            {
                var @event = _eventsDbSet
                    .Include(x => x.EventParticipants)
                    .Include(x => x.EventOptions)
                    .Include(x => x.EventType)
                    .Where(x => x.Id == joinDto.EventId
                        && x.OrganizationId == joinDto.OrganizationId)
                    .Select(MapEventToJoinValidationDto)
                    .FirstOrDefault();

                _eventValidationService.CheckIfEventExists(@event);

                @event.SelectedOptions = @event.Options
                    .Where(option => joinDto.ChosenOptions.Contains(option.Id))
                    .ToList();

                _eventValidationService.CheckIfRegistrationDeadlineIsExpired(@event.RegistrationDeadline);
                _eventValidationService.CheckIfProvidedOptionsAreValid(joinDto.ChosenOptions, @event.SelectedOptions);
                _eventValidationService.CheckIfJoiningNotEnoughChoicesProvided(@event.MaxChoices, joinDto.ChosenOptions.Count());
                _eventValidationService.CheckIfJoiningTooManyChoicesProvided(@event.MaxChoices, joinDto.ChosenOptions.Count());
                _eventValidationService.CheckIfSingleChoiceSelectedWithRule(@event.SelectedOptions, OptionRules.IgnoreSingleJoin);
                _eventValidationService.CheckIfEventHasEnoughPlaces(@event.MaxParticipants, @event.Participants.Count + joinDto.ParticipantIds.Count);

                foreach (var userId in joinDto.ParticipantIds)
                {
                    var userExists = _usersDbSet
                        .Any(x => x.Id == userId
                            && x.OrganizationId == joinDto.OrganizationId);
                    _eventValidationService.CheckIfUserExists(userExists);

                    var alreadyParticipates = @event.Participants.Any(p => p == userId);
                    _eventValidationService.CheckIfUserAlreadyJoinedSameEvent(alreadyParticipates);

                    ValidateSingleJoin(@event, userOrg: joinDto);
                    AddParticipant(userId, @event.Id, @event.SelectedOptions);

                    JoinLeaveEventWall(@event.ResponsibleUserId, userId, @event.WallId, joinDto);
                }

                _uow.SaveChanges(false);
                var choices = @event.SelectedOptions.Select(x => x.Option);
                _calendarService.AddParticipants(@event.Id, joinDto.OrganizationId, joinDto.ParticipantIds, choices);
            }
        }

        public void DeleteByEvent(Guid eventId, string userId)
        {
            var participants = _eventParticipantsDbSet
                .Where(p => p.EventId == eventId)
                .ToList();

            var timestamp = DateTime.UtcNow;
            foreach (var participant in participants)
            {
                participant.Modified = timestamp;
                participant.ModifiedBy = userId;
            }

            _uow.SaveChanges(false);

            participants.ForEach(p => _eventParticipantsDbSet.Remove(p));
            _uow.SaveChanges(false);
        }

        public IEnumerable<string> GetParticipantsEmailsIncludingHost(Guid eventId)
        {
            var emailsObj = _eventsDbSet
                .Include(e => e.EventParticipants)
                .Where(e => e.Id == eventId)
                .Select(e => new
                {
                    Participants = e.EventParticipants.Select(p => p.ApplicationUser.Email),
                    HostEmail = e.ResponsibleUser.Email
                })
                .Single();

            List<string> result = emailsObj.Participants.ToList();
            if (!result.Contains(emailsObj.HostEmail))
            {
                result.Add(emailsObj.HostEmail);
            }

            return result;
        }

        public IEnumerable<EventUserSearchResultDTO> SearchForEventJoinAutocomplete(Guid eventId, string searchString, UserAndOrganizationDTO userOrg)
        {
            var searchStringLowerCase = searchString.ToLowerInvariant();
            var participants = _eventsDbSet
                .Include(e => e.EventParticipants)
                .Where(e =>
                    e.Id == eventId &&
                    e.OrganizationId == userOrg.OrganizationId)
                .SelectMany(e => e.EventParticipants.Select(p => p.ApplicationUserId))
                .ToList();

#pragma warning disable S1449
            var users = _usersDbSet
                .Where(u =>
                    u.UserName.ToLower().StartsWith(searchStringLowerCase) ||
                    u.LastName.ToLower().StartsWith(searchStringLowerCase) ||
                    (u.FirstName + " " + u.LastName).ToLower().StartsWith(searchStringLowerCase))
                .Where(u =>
                    !participants.Contains(u.Id) &&
                    u.OrganizationId == userOrg.OrganizationId &&
                    u.Id != userOrg.UserId)
                .Where(_roleService.ExcludeUsersWithRole(Constants.Authorization.Roles.NewUser))
                .OrderBy(u => u.Id)
                .Select(u => new EventUserSearchResultDTO
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName
                })
                .ToList();
#pragma warning restore S1449

            return users;
        }

        public void Expel(Guid eventId, UserAndOrganizationDTO userOrg, string userId)
        {
            var participant = GetParticipant(eventId, userOrg.OrganizationId, userId);
            var @event = participant.Event;

            var isAdmin = _permissionService.UserHasPermission(userOrg, AdministrationPermissions.Event);
            _eventValidationService.CheckIfUserHasPermission(userOrg.UserId, @event.ResponsibleUserId, isAdmin);
            _eventValidationService.CheckIfEventEndDateIsExpired(@event.EndDate);

            RemoveParticipant(userId, participant);

            JoinLeaveEventWall(@event.ResponsibleUserId, userId, @event.WallId, userOrg);

            _asyncRunner.Run<IEventCalendarService>(n => n.RemoveParticipants(@event.Id, userOrg.OrganizationId, new[] { userId }), _uow.ConnectionName);
            _asyncRunner.Run<IEventNotificationService>(n => n.NotifyRemovedEventParticipants(@event.Name, @event.Id, userOrg.OrganizationId, new[] { userId }), _uow.ConnectionName);
        }

        public void Leave(Guid eventId, UserAndOrganizationDTO userOrg)
        {
            var participant = GetParticipant(eventId, userOrg.OrganizationId, userOrg.UserId);
            _eventValidationService.CheckIfRegistrationDeadlineIsExpired(participant.Event.RegistrationDeadline);

            JoinLeaveEventWall(participant.Event.ResponsibleUserId, userOrg.UserId, participant.Event.WallId, userOrg);

            RemoveParticipant(userOrg.UserId, participant);

            _calendarService.RemoveParticipants(eventId, userOrg.OrganizationId, new List<string> { userOrg.UserId });
        }

        public IEnumerable<EventParticipantDTO> GetEventParticipants(Guid eventId, UserAndOrganizationDTO userAndOrg)
        {
            var eventParticipants = _eventsDbSet
                .Include(e => e.EventParticipants.Select(x => x.ApplicationUser))
                .Where(e => e.Id == eventId
                    && e.OrganizationId == userAndOrg.OrganizationId)
                .Select(MapEventToParticipantDto())
                .SingleOrDefault();

            _eventValidationService.CheckIfEventExists(eventParticipants);
            _eventValidationService.CheckIfEventHasParticipants(eventParticipants);

            return eventParticipants;
        }

        public int GetMaxParticipantsCount(UserAndOrganizationDTO userAndOrganizationDTO)
        {
            var maxParticipantsCount = _usersDbSet
                .Where(_roleService.ExcludeUsersWithRole(Constants.Authorization.Roles.NewUser))
                .Where(x => x.OrganizationId == userAndOrganizationDTO.OrganizationId)
                .Count();

            return maxParticipantsCount;
        }

        private void JoinLeaveEventWall(string responsibleUserId, string wallParticipantId, int wallId, UserAndOrganizationDTO userOrg)
        {
            if (responsibleUserId != wallParticipantId)
            {
                _wallService.JoinLeaveWall(wallId, wallParticipantId, wallParticipantId, userOrg.OrganizationId, true);
            }
        }

        private void RemoveParticipant(string userId, EventParticipant participant)
        {
            var timestamp = DateTime.UtcNow;
            participant.UpdateMetadata(userId, timestamp);
            _uow.SaveChanges(false);

            _eventParticipantsDbSet.Remove(participant);
            _uow.SaveChanges(false);
        }

        private EventParticipant GetParticipant(Guid eventId, int userOrg, string userId)
        {
            var participant = _eventParticipantsDbSet
                            .Include(p => p.Event)
                            .Include(p => p.EventOptions)
                            .Where(p =>
                                p.EventId == eventId &&
                                p.Event.OrganizationId == userOrg &&
                                p.ApplicationUserId == userId)
                            .SingleOrDefault();

            _eventValidationService.CheckIfEventExists(participant);
            _eventValidationService.CheckIfParticipantExists(participant);

            return participant;
        }

        private Expression<Func<Event, IEnumerable<EventParticipantDTO>>> MapEventToParticipantDto()
        {
            return e => e.EventParticipants.Select(p => new EventParticipantDTO
            {
                FirstName = string.IsNullOrEmpty(p.ApplicationUser.FirstName)
                        ? ConstBusinessLayer.DeletedUserFirstName
                        : p.ApplicationUser.FirstName,

                LastName = string.IsNullOrEmpty(p.ApplicationUser.LastName)
                        ? ConstBusinessLayer.DeletedUserLastName
                        : p.ApplicationUser.LastName
            });
        }

        private Expression<Func<Event, EventJoinValidationDTO>> MapEventToJoinValidationDto =>
            e => new EventJoinValidationDTO
            {
                Participants = e.EventParticipants.Select(x => x.ApplicationUserId).ToList(),
                MaxParticipants = e.MaxParticipants,
                StartDate = e.StartDate,
                MaxChoices = e.MaxChoices,
                Id = e.Id,
                Options = e.EventOptions,
                IsSingleJoin = e.EventType.IsSingleJoin,
                EventTypeId = e.EventTypeId,
                Name = e.Name,
                EndDate = e.EndDate,
                RegistrationDeadline = e.RegistrationDeadline,
                ResponsibleUserId = e.ResponsibleUserId,
                WallId = e.WallId
            };

        private void ValidateSingleJoin(EventJoinValidationDTO eventDto, UserAndOrganizationDTO userOrg)
        {
            if (!eventDto.SelectedOptions.Any(x => x.Rule == OptionRules.IgnoreSingleJoin))
            {
                if (eventDto.IsSingleJoin)
                {
                    var events = _eventsDbSet
                        .Include(e => e.EventParticipants.Select(x => x.EventOptions))
                        .Where(x =>
                            x.EventTypeId == eventDto.EventTypeId &&
                            x.OrganizationId == userOrg.OrganizationId &&
                            x.EventParticipants.Any(p => p.ApplicationUserId == userOrg.UserId) &&
                            SqlFunctions.DatePart("wk", x.StartDate) == SqlFunctions.DatePart("wk", eventDto.StartDate))
                        .ToList();

                    var filteredEvents = RemoveEventsWithOptionRule(events, OptionRules.IgnoreSingleJoin);

                    _eventValidationService.CheckIfUserExistsInOtherSingleJoinEvent(filteredEvents);
                }
            }
        }

        private static IEnumerable<Event> RemoveEventsWithOptionRule(IList<Event> events, OptionRules rule)
        {
            var eventsToRemove = events.Where(x =>
                x.EventParticipants.Any(y => y.EventOptions.Any(z => z.Rule == rule)));

            return events.Except(eventsToRemove);
        }

        private void AddParticipant(string userId, Guid eventId, ICollection<EventOption> eventOptions)
        {
            var timeStamp = _systemClock.UtcNow;
            var newParticipant = new EventParticipant
            {
                ApplicationUserId = userId,
                Created = timeStamp,
                CreatedBy = userId,
                EventId = eventId,
                Modified = timeStamp,
                ModifiedBy = userId,
                EventOptions = eventOptions
            };
            _eventParticipantsDbSet.Add(newParticipant);
        }
    }
}