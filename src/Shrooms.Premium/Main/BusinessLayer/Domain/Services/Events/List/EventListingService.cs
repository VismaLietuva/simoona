using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Shrooms.DataTransferObjects.Models;
using Shrooms.EntityModels.Models.Events;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Premium.Constants;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events;
using Shrooms.Premium.Main.BusinessLayer.DomainServiceValidators.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.List
{
    public class EventListingService : IEventListingService
    {
        private static readonly Dictionary<BusinessLayerConstants.MyEventsOptions, Func<string, Expression<Func<Event, bool>>>>
            EventFilters = new Dictionary<BusinessLayerConstants.MyEventsOptions, Func<string, Expression<Func<Event, bool>>>>
        {
            { BusinessLayerConstants.MyEventsOptions.Host, MyEventsAsMasterFilter },
            { BusinessLayerConstants.MyEventsOptions.Participant, MyEventsAsParticipantFilter }
        };

        private readonly IEventValidationService _eventValidationService;

        private readonly IDbSet<Event> _eventsDbSet;

        public EventListingService(IUnitOfWork2 uow, IEventValidationService eventValidationService)
        {
            _eventValidationService = eventValidationService;
            _eventsDbSet = uow.GetDbSet<Event>();
        }

        public EventOptionsDTO GetEventOptions(Guid eventId, UserAndOrganizationDTO userOrg)
        {
            var eventOptionsDto = _eventsDbSet
                .Include(e => e.EventOptions)
                .Where(e =>
                    e.Id == eventId &&
                    e.OrganizationId == userOrg.OrganizationId)
                .Select(MapOptionsToDto())
                .SingleOrDefault();

            _eventValidationService.CheckIfEventExists(eventOptionsDto);
            return eventOptionsDto;
        }

        public IEnumerable<EventListItemDTO> GetEventsByType(UserAndOrganizationDTO userOrganization, int typeId = 0)
        {
            var events = _eventsDbSet
                .Include(x => x.EventParticipants)
                .Include(x => x.Office)
                .Where(t =>
                    t.OrganizationId == userOrganization.OrganizationId &
                    t.EndDate > DateTime.UtcNow)
                .Where(EventTypeFilter(typeId))
                .Select(MapEventToListItemDto(userOrganization.UserId))
                .OrderBy(e => e.StartDate)
                .ToList();
            return events;
        }

        public IEnumerable<EventListItemDTO> GetEventsByTypeAndOffice(UserAndOrganizationDTO userOrganization, int? typeId = null, int? officeId = null)
        {
            var events = _eventsDbSet
                .Include(x => x.EventParticipants)
                .Include(x => x.Office)
                .Where(t =>
                    t.OrganizationId == userOrganization.OrganizationId &
                    t.EndDate > DateTime.UtcNow)
                .Where(EventTypeFilter(typeId))
                .Where(EventOfficeFilter(officeId))
                .Select(MapEventToListItemDto(userOrganization.UserId))
                .OrderBy(e => e.StartDate)
                .ToList();
            return events;
        }

        public IEnumerable<EventListItemDTO> GetMyEvents(MyEventsOptionsDTO options, int? officeId = null)
        {
            var myEventFilter = EventFilters[options.Filter](options.UserId);
            var events = _eventsDbSet
                .Include(x => x.EventParticipants)
                .Include(x => x.Office)
                .Where(t => t.OrganizationId == options.OrganizationId)
                .Where(SearchFilter(options.SearchString))
                .Where(myEventFilter)
                .Where(EventOfficeFilter(officeId))
                .Select(MapEventToListItemDto(options.UserId))
                .OrderBy(e => e.StartDate)
                .ToList();

            var orderedEvents = OrderEvents(events);
            return orderedEvents;
        }

        private static Expression<Func<Event, bool>> MyEventsAsMasterFilter(string userId)
        {
            return x => x.ResponsibleUserId == userId;
        }

        private static Expression<Func<Event, bool>> MyEventsAsParticipantFilter(string userId)
        {
            return x => x.EventParticipants.Any(p => p.ApplicationUserId == userId);
        }

        private static IEnumerable<EventListItemDTO> OrderEvents(List<EventListItemDTO> events)
        {
            var orderedFutureEvents = events
                .Where(e => e.StartDate > DateTime.UtcNow)
                .OrderBy(e => e.StartDate);

            var orderedPastEvents = events
                .Where(e => e.StartDate < DateTime.UtcNow)
                .OrderByDescending(e => e.StartDate);
            return orderedFutureEvents.Concat(orderedPastEvents);
        }

        private static Expression<Func<Event, EventListItemDTO>> MapEventToListItemDto(string userId)
        {
            return e => new EventListItemDTO
            {
                Id = e.Id,
                ImageName = e.ImageName,
                MaxParticipants = e.MaxParticipants,
                Name = e.Name,
                Office = e.Office.Name,
                Place = e.Place,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                RegistrationDeadlineDate = e.RegistrationDeadline,
                ParticipantsCount = e.EventParticipants.Count,
                IsCreator = e.ResponsibleUserId == userId,
                IsParticipating = e.EventParticipants.Any(p => p.ApplicationUserId == userId),
                MaxChoices = e.MaxChoices
            };
        }

        private static Expression<Func<Event, EventOptionsDTO>> MapOptionsToDto()
        {
            return e => new EventOptionsDTO
            {
                MaxOptions = e.MaxChoices,
                Options = e.EventOptions.Select(o => new EventOptionDTO
                {
                    Id = o.Id,
                    Option = o.Option
                })
            };
        }

        private static Expression<Func<Event, bool>> EventTypeFilter(int? typeId)
        {
            if (typeId == null || typeId == 0)
            {
                return x => true;
            }

            return x => x.EventTypeId == typeId;
        }

        private static Expression<Func<Event, bool>> EventOfficeFilter(int? officeId)
        {
            if (officeId == null)
            {
                return x => true;
            }

            return x => x.OfficeId == officeId || x.OfficeId == null;
        }

        private static Expression<Func<Event, bool>> SearchFilter(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
            {
                return e => true;
            }

            return e => e.Name.Contains(searchString) || e.Place.Contains(searchString);
        }
    }
}
