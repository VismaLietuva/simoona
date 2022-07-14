using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;
using Shrooms.Domain.Extensions;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.Events.List
{
    public class EventListingService : IEventListingService
    {
        private const string OutsideOffice = "[]";

        private static readonly Dictionary<MyEventsOptions, Func<string, Expression<Func<Event, bool>>>>
            _eventFilters = new ()
            {
                { MyEventsOptions.Host, MyEventsAsMasterFilter },
                { MyEventsOptions.Participant, MyEventsAsParticipantFilter }
            };

        private readonly IEventValidationService _eventValidationService;

        private readonly IDbSet<Event> _eventsDbSet;
        private readonly DbSet<KudosLog> _kudosLogDbSet;
        private readonly DbSet<KudosType> _kudosTypesDbSet;
        private readonly DbSet<EventParticipant> _eventParticipantsDbSet;
        private readonly IDbSet<Office> _officeDbSet;

        public EventListingService(IUnitOfWork2 uow, IEventValidationService eventValidationService)
        {
            _eventValidationService = eventValidationService;
            _eventsDbSet = uow.GetDbSet<Event>();
            _kudosLogDbSet = uow.GetDbSet<KudosLog>();
            _kudosTypesDbSet = uow.GetDbSet<KudosType>();
            _eventParticipantsDbSet = uow.GetDbSet<EventParticipant>();
            _officeDbSet = uow.GetDbSet<Office>();
        }

        public async Task<EventOptionsDto> GetEventOptionsAsync(Guid eventId, UserAndOrganizationDto userOrg)
        {
            var eventOptionsDto = await _eventsDbSet
                .Include(e => e.EventOptions)
                .Where(e => e.Id == eventId && e.OrganizationId == userOrg.OrganizationId)
                .Select(MapOptionsToDto())
                .SingleOrDefaultAsync();

            _eventValidationService.CheckIfEventExists(eventOptionsDto);
            return eventOptionsDto;
        }

        public async Task<IEnumerable<EventListItemDto>> GetEventsFilteredAsync(EventFilteredArgsDto filteredArgsDto, UserAndOrganizationDto userOrganization)
        {
            var officeSearchString = OfficeIdToString(filteredArgsDto.OfficeIdParsed);

            var query = _eventsDbSet
                .Include(x => x.EventParticipants)
                .Include(x => x.EventType)
                .Where(e => e.OrganizationId == userOrganization.OrganizationId)
                .Where(EventTypeFilter(filteredArgsDto.TypeIdParsed, filteredArgsDto.IsOnlyMainEvents))
                .Where(EventOfficeFilter(officeSearchString));

            if (filteredArgsDto.StartDate is null || filteredArgsDto.EndDate is null)
            {
                query = query.Where(e => e.EndDate > DateTime.UtcNow);
            }
            else
            {
                _eventValidationService.CheckIfDateRangeExceededLimitOrNull(filteredArgsDto.StartDate, filteredArgsDto.EndDate);
                query = query.Where(e => e.StartDate >= filteredArgsDto.StartDate && e.EndDate <= filteredArgsDto.EndDate);
            }

            var events = query
                .OrderByDescending(e => e.IsPinned)
                .ThenBy(e => e.StartDate)
                .Skip((filteredArgsDto.Page - 1) * filteredArgsDto.PageSize)
                .Take(filteredArgsDto.PageSize)
                .Select(MapEventToListItemDto(userOrganization.UserId));

            return await events.ToListAsync();
        }

        public async Task<IPagedList<EventDetailsListItemDto>> GetNotStartedEventsFilteredByTitleAsync(EventReportListingArgsDto reportArgsDto, UserAndOrganizationDto userAndOrganization)
        {
            var allOffices = await _officeDbSet
                .ToDictionaryAsync(office => office.Id, office => office.Name);

            var officesCount = allOffices.Count;

            var typeIdsLength = reportArgsDto.EventTypeIds.Count();
            var officeIdsLength = reportArgsDto.OfficeTypeIds.Count();

            var events = await _eventsDbSet
                .Include(e => e.EventParticipants)
                .Include(e => e.EventType)
                .Where(e => (e.OrganizationId == userAndOrganization.OrganizationId) &&
                            (reportArgsDto.SearchString == null || e.Name.Contains(reportArgsDto.SearchString)) &&
                            (typeIdsLength == 0 || reportArgsDto.EventTypeIds.Contains(e.EventTypeId)) &&
                            (officeIdsLength == 0 || reportArgsDto.OfficeTypeIds.Any(c => e.Offices.Contains(c))))
                .Where(e => e.StartDate > DateTime.UtcNow)
                .OrderByPropertyNames(reportArgsDto)
                .Select(e => new EventDetailsListItemDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    TypeName = e.EventType.Name,
                    MaxParticipants = e.MaxParticipants,
                    ParticipantsCount = e.EventParticipants.Count,
                    Offices = e.Offices
                })
                .ToPagedListAsync(reportArgsDto.Page, reportArgsDto.PageSize);

            foreach (var e in events)
            {
                e.OfficeNames = e.OfficeIds
                    .Select(officeId => allOffices.TryGetValue(int.Parse(officeId), out var value) ? value : string.Empty)
                    .Where(name => name != string.Empty);

                e.IsForAllOffices = e.OfficeIds.Count() == officesCount;
            }

            return events;
        }
        
        public async Task<IPagedList<EventVisitedReportDto>> GetEventParticipantVisitedReportEventsAsync(EventParticipantVisitedEventsListingArgsDto visitedArgsDto, UserAndOrganizationDto userOrg)
        {
            var eventTypesLength = visitedArgsDto.EventTypeIds.Count();
            var visitedEvents = await _eventsDbSet
                .Include(e => e.EventParticipants)
                .Where(e => e.EventParticipants.Any(participant => participant.ApplicationUserId == visitedArgsDto.UserId) &&
                                    e.EndDate < DateTime.UtcNow &&
                                    e.OrganizationId == userOrg.OrganizationId &&
                                    (eventTypesLength == 0 || visitedArgsDto.EventTypeIds.Contains(e.EventType.Id)) &&
                                    e.StartDate >= visitedArgsDto.StartDate && 
                                    e.EndDate <= visitedArgsDto.EndDate)
                .Select(e => new EventVisitedReportDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    TypeName = e.EventType.Name,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate
                })
                .OrderByPropertyNames(visitedArgsDto)
                .ToPagedListAsync(visitedArgsDto.Page, visitedArgsDto.PageSize);

            return visitedEvents;
        }

        public async Task<IPagedList<EventParticipantReportDto>> GetReportParticipantsAsync(EventParticipantsReportListingArgsDto reportArgsDto, UserAndOrganizationDto userOrg)
        {
            var kudosTypesLength = reportArgsDto.KudosTypeIds.Count();
            var eventTypesLength = reportArgsDto.EventTypeIds.Count();

            var kudosTypeNames = await _kudosTypesDbSet
                .Where(type => reportArgsDto.KudosTypeIds.Contains(type.Id))
                .Select(type => type.Name)
                .ToListAsync();

            return await _eventParticipantsDbSet
                .Include(p => p.ApplicationUser)
                .Where(p => p.EventId == reportArgsDto.EventId &&
                            p.ApplicationUser.OrganizationId == userOrg.OrganizationId)
                .Select(p => new EventParticipantReportDto
                {
                    Id = p.ApplicationUser.Id,
                    FirstName = p.ApplicationUser.FirstName,
                    LastName = p.ApplicationUser.LastName,
                    EmploymentDate = p.ApplicationUser.EmploymentDate,
                    QualificationLevel = p.ApplicationUser.QualificationLevel.Name,
                    JobTitle = p.ApplicationUser.JobPosition.Title,
                    ManagerFirstName = p.ApplicationUser.Manager.FirstName,
                    ManagerLastName = p.ApplicationUser.Manager.LastName,
                    ManagerId = p.ApplicationUser.Manager.Id,
                    Projects = p.ApplicationUser.Projects
                        .Select(pr => new EventProjectReportDto
                        {
                            Id = pr.Id,
                            Name = pr.Name
                        })
                        .ToList(),
                    Kudos = _kudosLogDbSet
                        .Where(kudos =>
                            (kudosTypesLength == 0 || kudosTypeNames.Contains(kudos.KudosTypeName)) &&
                            (kudos.EmployeeId == p.ApplicationUser.Id) &&
                            (kudos.Status == KudosStatus.Approved) &&
                            (kudos.OrganizationId == userOrg.OrganizationId))
                        .Sum(kudos => kudos.Points),
                    VisitedEvents = _eventsDbSet
                        .Where(e => e.EventParticipants.Any(participant => participant.ApplicationUserId == p.ApplicationUser.Id) &&
                                    e.EndDate < DateTime.UtcNow &&
                                    e.OrganizationId == userOrg.OrganizationId &&
                                    (eventTypesLength == 0 || reportArgsDto.EventTypeIds.Contains(e.EventType.Id)))
                        .OrderByDescending(e => e.EndDate)
                        .Take(EventsConstants.EventReportVisitedEventPreviewCount)
                        .Select(visited => new EventVisitedReportDto
                        {
                            Id = visited.Id,
                            Name = visited.Name,
                            TypeName = visited.EventType.Name,
                            StartDate = visited.StartDate,
                            EndDate = visited.EndDate
                        })
                        .ToList(),
                    TotalVisitedEventCount = _eventsDbSet
                        .Count(e => e.EventParticipants.Any(participant => participant.ApplicationUserId == p.ApplicationUser.Id) &&
                                    e.EndDate < DateTime.UtcNow &&
                                    e.OrganizationId == userOrg.OrganizationId &&
                                    (eventTypesLength == 0 || reportArgsDto.EventTypeIds.Contains(e.EventType.Id)))
                })
                .OrderByPropertyNames(reportArgsDto)
                .ToPagedListAsync(reportArgsDto.Page, reportArgsDto.PageSize);
        }

        public async Task<IEnumerable<EventListItemDto>> GetMyEventsAsync(MyEventsOptionsDto options, UserAndOrganizationDto userOrg, int? officeIdNullable = null)
        {
            var officeSearchString = OfficeIdToString(officeIdNullable);
            var myEventFilter = _eventFilters[options.Filter](userOrg.UserId);
            var events = await _eventsDbSet
                .Include(x => x.EventParticipants)
                .Include(x => x.Offices)
                .Where(t => t.OrganizationId == userOrg.OrganizationId)
                .Where(SearchFilter(options.SearchString))
                .Where(myEventFilter)
                .Where(EventOfficeFilter(officeSearchString))
                .OrderByDescending(e => e.StartDate)
                .Skip((options.Page - 1) * options.PageSize)
                .Take(options.PageSize)
                .Select(MapEventToListItemDto(userOrg.UserId))
                .ToListAsync();

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

        private static IEnumerable<EventListItemDto> OrderEvents(List<EventListItemDto> events)
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

        private static Expression<Func<Event, EventListItemDto>> MapEventToListItemDto(string userId)
        {
            return e => new EventListItemDto
            {
                Id = e.Id,
                ImageName = e.ImageName,
                Offices = new EventOfficesDto { Value = e.Offices },
                MaxParticipants = e.MaxParticipants,
                IsPinned = e.IsPinned,
                Name = e.Name,
                Place = e.Place,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                RegistrationDeadlineDate = e.RegistrationDeadline,
                ParticipantsCount = e.EventParticipants.Count(p => p.AttendStatus == (int)AttendingStatus.Attending),
                IsCreator = e.ResponsibleUserId == userId,
                ParticipatingStatus = e.EventParticipants.FirstOrDefault(p => p.ApplicationUserId == userId) != null
                    ? e.EventParticipants.FirstOrDefault(p => p.ApplicationUserId == userId).AttendStatus
                    : (int)AttendingStatus.Idle,
                MaxChoices = e.MaxChoices
            };
        }

        private static Expression<Func<Event, EventOptionsDto>> MapOptionsToDto()
        {
            return e => new EventOptionsDto
            {
                MaxOptions = e.MaxChoices,
                Options = e.EventOptions.Select(o => new EventOptionDto
                    {
                        Id = o.Id,
                        Option = o.Option,
                        Rule = o.Rule
                    })
                    .OrderByDescending(o => o.Rule == OptionRules.Default)
            };
        }

        private static Expression<Func<Event, bool>> EventTypeFilter(int? typeId, bool includeOnlyMain = false)
        {
            if (includeOnlyMain)
            {
                return x => x.EventType.IsShownWithMainEvents;
            }

            if (typeId == null || typeId == 0)
            {
                return x => true;
            }

            return x => x.EventTypeId == typeId;
        }

        private static Expression<Func<Event, bool>> EventOfficeFilter(string office)
        {
            if (office == OutsideOffice || office == null)
            {
                return x => true;
            }

            return x => x.Offices.Contains(office) || x.Offices == OutsideOffice;
        }

        private static Expression<Func<Event, bool>> SearchFilter(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
            {
                return e => true;
            }

            return e => e.Name.Contains(searchString) || e.Place.Contains(searchString);
        }

        private static string OfficeIdToString(int? officeId) => officeId != null ? $@"""{officeId.ToString()}""" : OutsideOffice;
    }
}
