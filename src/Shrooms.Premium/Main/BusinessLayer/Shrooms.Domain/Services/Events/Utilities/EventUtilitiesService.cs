using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models.Events;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.Premium.Other.Shrooms.Constants.ErrorCodes;
using static Shrooms.Constants.ErrorCodes.ErrorCodes;

namespace Shrooms.Domain.Services.Events.Utilities
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
                    IsFoodRelated = type.IsFoodRelated,
                    Name = type.Name
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
                    IsFoodRelated = x.IsFoodRelated,
                    Name = x.Name,
                    HasActiveEvents = x.Events.Any(e => e.EndDate > DateTime.UtcNow 
                                                     || e.EventRecurring != EventRecurrenceOptions.None)
                })
                .SingleOrDefault();

            if (eventType == null)
            {
                throw new ValidationException(ContentDoesNotExist, "Event type does not exist");
            }

            return eventType;
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
                throw new ValidationException(ContentDoesNotExist, "Event type does not exist");
            }

            orgEventType.IsSingleJoin = eventType.IsSingleJoin;
            orgEventType.IsFoodRelated = eventType.IsFoodRelated;
            orgEventType.Name = eventType.Name;
            orgEventType.ModifiedBy = eventType.UserId;
            orgEventType.Modified = DateTime.UtcNow;

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
                throw new ValidationException(ContentDoesNotExist, "Event type has active events");
            }

            var eventType = _eventTypesDbSet
                .SingleOrDefault(x => x.OrganizationId == userAndOrg.OrganizationId 
                    && x.Id == id);

            if (eventType == null)
            {
                throw new ValidationException(ContentDoesNotExist, "Event type does not exist");
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

        private void ValidateEventTypeName(string eventTypeName, int organizationId)
        {
            var nameAlreadyExists = _eventTypesDbSet
                            .Any(x => x.OrganizationId == organizationId
                                && x.Name == eventTypeName);

            if (nameAlreadyExists)
            {
                throw new ValidationException(ErrorCodes.EventTypeNameAlreadyExists, "Event type name should be unique");
            }
        }

        private EventType MapNewEventType(CreateEventTypeDTO eventTypeDto)
        {
            var eventType = new EventType()
            {
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                CreatedBy = eventTypeDto.UserId,
                OrganizationId = eventTypeDto.OrganizationId,
                IsSingleJoin = eventTypeDto.IsSingleJoin,
                IsFoodRelated = eventTypeDto.IsFoodRelated,
                Name = eventTypeDto.Name
            };

            return eventType;
        }
    }
}