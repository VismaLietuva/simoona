using Newtonsoft.Json;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.DomainServiceValidators.Validators.Events;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Events;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Shrooms.Domain.Services.Events.List
{
    public class EventListingService : IEventListingService
    {
        private static readonly Dictionary<ConstBusinessLayer.MyEventsOptions, Func<string, Expression<Func<Event, bool>>>>
            EventFilters = new Dictionary<ConstBusinessLayer.MyEventsOptions, Func<string, Expression<Func<Event, bool>>>>
        {
            { ConstBusinessLayer.MyEventsOptions.Host, MyEventsAsMasterFilter },
            { ConstBusinessLayer.MyEventsOptions.Participant, MyEventsAsParticipantFilter }
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
                .Where(t =>
                    t.OrganizationId == userOrganization.OrganizationId &
                    t.EndDate > DateTime.UtcNow)
                .Where(EventTypeFilter(typeId))
                .Select(MapEventToListItemDto(userOrganization.UserId))
                .OrderByDescending(e => e.IsPinned)
                .ThenBy(e => e.StartDate)
                .ToList();


            return events;
        }

        public IEnumerable<EventListItemDTO> GetEventsByTypeAndOffice(UserAndOrganizationDTO userOrganization, int? typeId = null, int? officeId = null)
        {
            string officeSearchString = officeId != null ? $"\"{officeId.ToString()}\"" : "[]";
            IList < EventListItemDTO > events = _eventsDbSet
                .Include(x => x.EventParticipants)
                .Where(t =>
                    t.OrganizationId == userOrganization.OrganizationId &
                    t.EndDate > DateTime.UtcNow)
                .Where(EventTypeFilter(typeId))
                .Where(EventOfficeFilter(officeSearchString))
                .Select(MapEventToListItemDto(userOrganization.UserId))
                .OrderByDescending(e => e.IsPinned)
                .ThenBy(e => e.StartDate)
                .ToList();
            
            return events;
        }

        public IEnumerable<EventListItemDTO> GetMyEvents(MyEventsOptionsDTO options, int? officeId = null)
        {
            string officeSearchString = officeId != null ? $"\"{officeId.ToString()}\"" : "[]";
            var myEventFilter = EventFilters[options.Filter](options.UserId);
            var events = _eventsDbSet
                .Include(x => x.EventParticipants)
                .Include(x => x.Offices)
                .Where(t => t.OrganizationId == options.OrganizationId)
                .Where(SearchFilter(options.SearchString))
                .Where(myEventFilter)
                .Where(EventOfficeFilter(officeSearchString))
                .Select(MapEventToListItemDto(options.UserId))
                .OrderByDescending(e => e.IsPinned)
                .ThenBy(e => e.StartDate)
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
                .OrderByDescending(e => e.IsPinned)
                .ThenBy(e => e.StartDate);

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
                Offices = new EventOfficesDTO { Value = e.Offices},
                MaxParticipants = e.MaxParticipants,
                IsPinned = e.IsPinned,
                Name = e.Name,
                Place = e.Place,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                RegistrationDeadlineDate = e.RegistrationDeadline,
                ParticipantsCount = e.EventParticipants.Where(p => p.AttendStatus == (int)ConstBusinessLayer.AttendingStatus.Attending).Count(),
                IsCreator = e.ResponsibleUserId == userId,
                ParticipatingStatus = e.EventParticipants.FirstOrDefault(p => p.ApplicationUserId == userId) != null ? e.EventParticipants.FirstOrDefault(p => p.ApplicationUserId == userId).AttendStatus : (int)ConstBusinessLayer.AttendingStatus.Idle,
                MaxChoices = e.MaxChoices,
                FoodOption = e.FoodOption
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

        private static Expression<Func<Event, bool>> EventOfficeFilter(string office)
        {
            if (office == "[]")
            {
                return x => true;
            }
            return x => x.Offices.Contains(office) || x.Offices == "[]";
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
