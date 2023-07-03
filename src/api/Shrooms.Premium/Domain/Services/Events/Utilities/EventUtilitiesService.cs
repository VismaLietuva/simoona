using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Services.FilterPresets;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Events.Utilities
{
    public class EventUtilitiesService : IEventUtilitiesService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<Event> _eventsDbSet;
        private readonly IDbSet<EventType> _eventTypesDbSet;
        private readonly IDbSet<EventOption> _eventOptionsDbSet;

        private readonly IFilterPresetService _filterPresetService;

        public EventUtilitiesService(
            IUnitOfWork2 uow,
            IFilterPresetService filterPresetService)
        {
            _uow = uow;
            _eventsDbSet = uow.GetDbSet<Event>();
            _eventTypesDbSet = uow.GetDbSet<EventType>();
            _eventOptionsDbSet = uow.GetDbSet<EventOption>();

            _filterPresetService = filterPresetService;
        }

        public async Task DeleteEventOptionsAsync(Guid eventId, string userId)
        {
            var options = await _eventOptionsDbSet
                .Where(o => o.EventId == eventId)
                .ToListAsync();

            var timestamp = DateTime.UtcNow;
            foreach (var option in options)
            {
                option.Modified = timestamp;
                option.ModifiedBy = userId;
            }

            await _uow.SaveChangesAsync(false);

            options.ForEach(o => _eventOptionsDbSet.Remove(o));
            await _uow.SaveChangesAsync(false);
        }

        public async Task<string> GetEventNameAsync(Guid eventId)
        {
            return (await _eventsDbSet.SingleAsync(e => e.Id == eventId)).Name;
        }

        public async Task<IEnumerable<EventTypeDto>> GetEventTypesAsync(int organizationId)
        {
            var eventTypes = await _eventTypesDbSet
                .Where(x => x.OrganizationId == organizationId)
                .Select(type => new EventTypeDto
                {
                    Id = type.Id,
                    IsSingleJoin = type.IsSingleJoin,
                    Name = type.Name,
                    IsShownWithMainEvents = type.IsShownWithMainEvents,
                    CanBeDisplayedInUpcomingEventsWidget = type.CanBeDisplayedInUpcomingEventsWidget
                })
                .OrderByDescending(t => t.Name)
                .ToListAsync();

            return eventTypes;
        }

        public async Task<EventTypeDto> GetEventTypeAsync(int organizationId, int eventTypeId)
        {
            var eventType = await _eventTypesDbSet
                .Include(x => x.Events)
                .Where(x => x.OrganizationId == organizationId && x.Id == eventTypeId)
                .Select(x => new EventTypeDto
                {
                    Id = x.Id,
                    IsSingleJoin = x.IsSingleJoin,
                    SendWeeklyReminders = x.SendWeeklyReminders,
                    Name = x.Name,
                    SingleJoinGroupName = x.SingleJoinGroupName,
                    IsShownWithMainEvents = x.IsShownWithMainEvents,
                    CanBeDisplayedInUpcomingEventsWidget = x.CanBeDisplayedInUpcomingEventsWidget,
                    SendEmailToManager = x.SendEmailToManager,
                    HasActiveEvents = x.Events.Any(e => e.EndDate > DateTime.UtcNow
                                                     || e.EventRecurring != EventRecurrenceOptions.None)
                })
                .SingleOrDefaultAsync();

            if (eventType == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Event type does not exist");
            }

            return eventType;
        }

        public async Task<IEnumerable<EventTypeDto>> GetEventTypesToRemindAsync(int organizationId)
        {
            return await _eventTypesDbSet
                .Where(x => x.SendWeeklyReminders && x.OrganizationId == organizationId)
                .Select(x => new EventTypeDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();
        }

        public async Task CreateEventTypeAsync(CreateEventTypeDto eventType)
        {
            await ValidateEventTypeNameAsync(eventType.Name, eventType.OrganizationId);

            var entity = MapNewEventType(eventType);
            _eventTypesDbSet.Add(entity);
            await _uow.SaveChangesAsync(eventType.UserId);
        }

        public async Task UpdateEventTypeAsync(UpdateEventTypeDto eventType)
        {
            var orgEventType = await _eventTypesDbSet
                .SingleOrDefaultAsync(x => x.OrganizationId == eventType.OrganizationId && x.Id == eventType.Id);

            if (orgEventType == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Event type does not exist");
            }

            if (eventType.Name != orgEventType.Name)
            {
                await ValidateEventTypeNameAsync(eventType.Name, eventType.OrganizationId);
            }

            orgEventType.IsSingleJoin = eventType.IsSingleJoin;
            orgEventType.Name = eventType.Name;
            orgEventType.SingleJoinGroupName = SetSingleJoinGroupName(eventType.IsSingleJoin, eventType.SingleJoinGroupName);
            orgEventType.ModifiedBy = eventType.UserId;
            orgEventType.Modified = DateTime.UtcNow;
            orgEventType.SendWeeklyReminders = eventType.SendWeeklyReminders;
            orgEventType.IsShownWithMainEvents = eventType.IsShownWithMainEvents;
            orgEventType.SendEmailToManager = eventType.SendEmailToManager;
            orgEventType.CanBeDisplayedInUpcomingEventsWidget = eventType.CanBeDisplayedInUpcomingEventsWidget;

            await _uow.SaveChangesAsync(eventType.UserId);
        }

        public async Task DeleteEventTypeAsync(int id, UserAndOrganizationDto userAndOrg)
        {
            var anyActiveEvents = await _eventsDbSet
                .Where(x => x.EventTypeId == id)
                .AnyAsync(x => x.EndDate > DateTime.UtcNow || x.EventRecurring != EventRecurrenceOptions.None);

            if (anyActiveEvents)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Event type has active events");
            }

            var eventType = await _eventTypesDbSet.SingleOrDefaultAsync(x => x.OrganizationId == userAndOrg.OrganizationId && x.Id == id);

            if (eventType == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Event type does not exist");
            }

            var eventTypeId = eventType.Id;

            _eventTypesDbSet.Remove(eventType);

            await _uow.SaveChangesAsync(userAndOrg.UserId);

            await _filterPresetService.RemoveDeletedTypeFromPresetsAsync(eventTypeId.ToString(), FilterType.Events, userAndOrg.OrganizationId);
        }

        public IEnumerable<object> GetRecurrenceOptions()
        {
            var recurrenceOptions = Enum
                .GetValues(typeof(EventRecurrenceOptions))
                .Cast<EventRecurrenceOptions>()
                .Select(o => new { Key = o.ToString(), Value = o });
            return recurrenceOptions;
        }

        public async Task<IEnumerable<EventOptionCountDto>> GetEventChosenOptionsAsync(Guid eventId, UserAndOrganizationDto userAndOrg)
        {
            var eventOptions = await _eventOptionsDbSet
                .Include(e => e.EventParticipants)
                .Include(e => e.Event)
                .Where(e => e.EventId == eventId
                    && e.Event.OrganizationId == userAndOrg.OrganizationId
                    && e.EventParticipants.Any(x => x.EventId == eventId))
                .Select(e => new EventOptionCountDto
                {
                    Option = e.Option,
                    Count = e.EventParticipants.Count(x => x.EventId == eventId)
                })
                .ToListAsync();

            return eventOptions;
        }

        public async Task<bool> AnyEventsThisWeekByTypeAsync(IEnumerable<int> eventTypeIds)
        {
            return await _eventsDbSet
                .AnyAsync(x => SqlFunctions.DatePart("wk", x.StartDate) == SqlFunctions.DatePart("wk", DateTime.UtcNow) &&
                          eventTypeIds.Contains(x.EventType.Id) &&
                          x.RegistrationDeadline > DateTime.UtcNow);
        }

        public async Task<IEnumerable<string>> GetEventTypesSingleJoinGroupsAsync(int organizationId)
        {
            return await _eventTypesDbSet
                .Where(x => x.OrganizationId == organizationId &&
                            !string.IsNullOrEmpty(x.SingleJoinGroupName))
                .Select(x => x.SingleJoinGroupName)
                .Distinct()
                .ToListAsync();
        }

        private async Task ValidateEventTypeNameAsync(string eventTypeName, int organizationId)
        {
            var nameAlreadyExists = await _eventTypesDbSet
                            .AnyAsync(x => x.OrganizationId == organizationId && x.Name == eventTypeName);

            if (nameAlreadyExists)
            {
                throw new ValidationException(PremiumErrorCodes.EventTypeNameAlreadyExists, "Event type name should be unique");
            }
        }

        private static EventType MapNewEventType(CreateEventTypeDto eventTypeDto)
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
                IsShownWithMainEvents = eventTypeDto.IsShownWithMainEvents,
                SendEmailToManager = eventTypeDto.SendEmailToManager,
                CanBeDisplayedInUpcomingEventsWidget = eventTypeDto.CanBeDisplayedInUpcomingEventsWidget
            };

            return eventType;
        }

        private static string SetSingleJoinGroupName(bool isSingleJoin, string groupName)
        {
            return isSingleJoin && !string.IsNullOrEmpty(groupName) ? groupName : null;
        }
    }
}
