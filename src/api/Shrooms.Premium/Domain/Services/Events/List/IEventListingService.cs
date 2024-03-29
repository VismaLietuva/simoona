﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.Events.List
{
    public interface IEventListingService
    {
        Task<IEnumerable<EventListItemDto>> GetMyEventsAsync(MyEventsOptionsDto options, UserAndOrganizationDto userOrg, int? officeIdNullable = null);

        Task<EventOptionsDto> GetEventOptionsAsync(Guid eventId, UserAndOrganizationDto userOrg);

        Task<IEnumerable<EventListItemDto>> GetEventsFilteredAsync(EventFilteredArgsDto filteredArgsDto, UserAndOrganizationDto userOrganization);

        Task<IPagedList<EventDetailsListItemDto>> GetNotStartedEventsFilteredByTitleAsync(EventReportListingArgsDto reportArgsDto, UserAndOrganizationDto userAndOrganization);

        Task<IPagedList<EventParticipantReportDto>> GetReportParticipantsAsync(EventParticipantsReportListingArgsDto reportArgsDto, UserAndOrganizationDto userOrg);

        Task<IPagedList<EventVisitedReportDto>> GetEventParticipantVisitedReportEventsAsync(EventParticipantVisitedEventsListingArgsDto visitedArgsDto, UserAndOrganizationDto userOrg);
    }
}
