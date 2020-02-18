using System;
using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.Domain.Services.Args;

namespace Shrooms.Domain.Services.Events.List
{
    public interface IEventListingService
    {
        IEnumerable<EventListItemDTO> GetMyEvents(MyEventsOptionsDTO options, int page, int? officeId = null);
        EventOptionsDTO GetEventOptions(Guid eventId, UserAndOrganizationDTO userOrg);
        IEnumerable<EventListItemDTO> GetEventsByType(UserAndOrganizationDTO userOrganization, int typeId = 0);
        IEnumerable<EventListItemDTO> GetEventsByTypeAndOffice(EventsListingFilterArgs args, UserAndOrganizationDTO userOrganization);
    }
}
