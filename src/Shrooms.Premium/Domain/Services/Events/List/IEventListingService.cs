using System;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.Services.Args;

namespace Shrooms.Premium.Domain.Services.Events.List
{
    public interface IEventListingService
    {
        IEnumerable<EventListItemDTO> GetMyEvents(MyEventsOptionsDTO options, int page, int? officeId = null);
        EventOptionsDTO GetEventOptions(Guid eventId, UserAndOrganizationDTO userOrg);
        IEnumerable<EventListItemDTO> GetEventsFiltered(EventsListingFilterArgs args, UserAndOrganizationDTO userOrganization);
    }
}
