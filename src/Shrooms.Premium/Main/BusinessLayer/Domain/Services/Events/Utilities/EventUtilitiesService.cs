using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using Shrooms.DataTransferObjects.Models;
using Shrooms.EntityModels.Models.Events;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Host.Contracts.Exceptions;
using Shrooms.Premium.Constants;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.Utilities
{
    public class EventUtilitiesService : IEventUtilitiesService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<Event> _eventsDbSet;
        private readonly IDbSet<EventType> _eventTypesDbSet;
        private readonly IDbSet<EventOption> _eventOptionsDbSet;

        public EventUtilitiesService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _eventsDbSet = uow.GetDbSet<Event>();
            _eventTypesDbSet = uow.GetDbSet<EventType>();
            _eventOptionsDbSet = uow.GetDbSet<EventOption>();
        }

        public void DeleteByEvent(Guid eventId, string userId)
        {
            var options = _eventOptionsDbSet
                .Where(o => o.EventId == eventId)
                .ToList();

            var timestamp = DateTime.UtcNow;
            foreach (var option in options)
            {
                option.Modified = timestamp;
                option.ModifiedBy = userId;
            }

            _uow.SaveChanges(false);

            options.ForEach(o => _eventOptionsDbSet.Remove(o));
            _uow.SaveChanges(false);
        }

        public string GetEventName(Guid eventId)
        {
            return _eventsDbSet
                .Single(e => e.Id == eventId)
                .Name;
        }

        public IEnumerable<EventTypeDTO> GetEventTypes(int organizationId)
        {
            var eventTypes = _eventTypesDbSet
                .Where(x => x.OrganizationId == organizationId)
                .Select(type => new EventTypeDTO
                {
                    Id = type.Id,
                    IsSingleJoin = type.IsSingleJoin,
                    Name = type.Name,
                    IsShownWithMainEvents = type.IsShownWithMainEvents
                })
                .OrderByDescending(t => t.Name)
                .ToList();
            return eventTypes;
        }

        public EventTypeDTO GetEventType(int organizationId, int eventTypeId)
        {
            var eventType = _eventTypesDbSet
                .Include(x => x.Events)
                .Where(x => x.OrganizationId == organizationId && x.Id == eventTypeId)
                .Select(x => new EventTypeDTO
                {
                    Id = x.Id,
                    IsSingleJoin = x.IsSingleJoin,
                    SendWeeklyReminders = x.SendWeeklyReminders,
                    Name = x.Name,
                    SingleJoinGroupName = x.SingleJoinGroupName,
                    IsShownWithMainEvents = x.IsShownWithMainEvents,
                    HasActiveEvents = x.Events.Any(e => e.EndDate > DateTime.UtcNow
                                                     || e.EventRecurring != EventRecurrenceOptions.None)
                })
                .SingleOrDefault();

            if (eventType == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Event type does not exist");
            }

            return eventType;
        }

        public IEnumerable<EventTypeDTO> GetEventTypesToRemind(int organizationId)
        {
            return _eventTypesDbSet
                .Where(x => x.SendWeeklyReminders && x.OrganizationId == organizationId)
                .Select(x => new EventTypeDTO
                {
                    Id = x.Id,
                    Name = x.Name
                });
        }

        public void CreateEventType(CreateEventTypeDTO eventType)
        {
            ValidateEventTypeName(eventType.Name, eventType.OrganizationId);

            var entity = MapNewEventType(eventType);
            _eventTypesDbSet.Add(entity);
            _uow.SaveChanges(eventType.UserId);
        }

        public void UpdateEventType(UpdateEventTypeDTO eventType)
        {
            var orgEventType = _eventTypesDbSet
                .SingleOrDefault(x => x.OrganizationId == eventType.OrganizationId && x.Id == eventType.Id);

            if (orgEventType == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Event type does not exist");
            }

            if (eventType.Name != orgEventType.Name)
            {
                ValidateEventTypeName(eventType.Name, eventType.OrganizationId);
            }

            orgEventType.IsSingleJoin = eventType.IsSingleJoin;
            orgEventType.Name = eventType.Name;
            orgEventType.SingleJoinGroupName = SetSingleJoinGroupName(eventType.IsSingleJoin, eventType.SingleJoinGroupName);
            orgEventType.ModifiedBy = eventType.UserId;
            orgEventType.Modified = DateTime.UtcNow;
            orgEventType.SendWeeklyReminders = eventType.SendWeeklyReminders;
            orgEventType.IsShownWithMainEvents = eventType.IsShownWithMainEvents;

            _uow.SaveChanges(eventType.UserId);
        }

        public void DeleteEventType(int id, UserAndOrganizationDTO userAndOrg)
        {
            var anyActiveEvents = _eventsDbSet
                .Where(x => x.EventTypeId == id)
                .Any(x => x.EndDate > DateTime.UtcNow
                    || x.EventRecurring != EventRecurrenceOptions.None);

            if (anyActiveEvents)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Event type has active events");
            }

            var eventType = _eventTypesDbSet
                .SingleOrDefault(x => x.OrganizationId == userAndOrg.OrganizationId
                    && x.Id == id);

            if (eventType == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Event type does not exist");
            }

            _eventTypesDbSet.Remove(eventType);
            _uow.SaveChanges(userAndOrg.UserId);
        }

        public IEnumerable<object> GetRecurranceOptions()
        {
            var recurranceOptions = Enum
                .GetValues(typeof(EventRecurrenceOptions))
                .Cast<EventRecurrenceOptions>()
                .Select(o => new { Key = o.ToString(), Value = o });
            return recurranceOptions;
        }

        public IEnumerable<EventOptionCountDTO> GetEventChosenOptions(Guid eventId, UserAndOrganizationDTO userAndOrg)
        {
            var eventOptions = _eventOptionsDbSet
                .Include(e => e.EventParticipants)
                .Include(e => e.Event)
                .Where(e => e.EventId == eventId
                    && e.Event.OrganizationId == userAndOrg.OrganizationId
                    && e.EventParticipants.Any(x => x.EventId == eventId))
                .Select(e => new EventOptionCountDTO
                {
                    Option = e.Option,
                    Count = e.EventParticipants.Count(x => x.EventId == eventId)
                })
                .ToList();

            return eventOptions;
        }

        public bool AnyEventsThisWeekByType(IEnumerable<int> eventTypeIds)
        {
            return _eventsDbSet
                .Any(x => SqlFunctions.DatePart("wk", x.StartDate) == SqlFunctions.DatePart("wk", DateTime.UtcNow) &&
                          eventTypeIds.Contains(x.EventType.Id) &&
                          x.RegistrationDeadline > DateTime.UtcNow);
        }

        public IEnumerable<string> GetEventTypesSingleJoinGroups(int organizationId)
        {
            return _eventTypesDbSet
                .Where(x => x.OrganizationId == organizationId &&
                            !string.IsNullOrEmpty(x.SingleJoinGroupName))
                .Select(x => x.SingleJoinGroupName)
                .Distinct();
        }

        private void ValidateEventTypeName(string eventTypeName, int organizationId)
        {
            var nameAlreadyExists = _eventTypesDbSet
                            .Any(x => x.OrganizationId == organizationId
                                && x.Name == eventTypeName);

            if (nameAlreadyExists)
            {
                throw new ValidationException(PremiumErrorCodes.EventTypeNameAlreadyExists, "Event type name should be unique");
            }
        }

        private static EventType MapNewEventType(CreateEventTypeDTO eventTypeDto)
        {
            var eventType = new EventType
            {
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                CreatedBy = eventTypeDto.UserId,
                OrganizationId = eventTypeDto.OrganizationId,
                IsSingleJoin = eventTypeDto.IsSingleJoin,
                SendWeeklyReminders = eventTypeDto.SendWeeklyReminders,
                Name = eventTypeDto.Name,
                SingleJoinGroupName = SetSingleJoinGroupName(eventTypeDto.IsSingleJoin, eventTypeDto.SingleJoinGroupName),
                IsShownWithMainEvents = eventTypeDto.IsShownWithMainEvents
            };

            return eventType;
        }

        private static string SetSingleJoinGroupName(bool isSingleJoin, string groupName)
        {
            return isSingleJoin && !string.IsNullOrEmpty(groupName) ? groupName : null;
        }
    }
}