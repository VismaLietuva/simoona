using System;
using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Events;

namespace Shrooms.Domain.Services.Events.List
{
    public interface IEventListingService
    {
        IEnumerable<EventListItemDTO> GetMyEvents(MyEventsOptionsDTO options);
        EventOptionsDTO GetEventOptions(Guid eventId, UserAndOrganizationDTO userOrg);
        IEnumerable<EventListItemDTO> GetEventsByType(UserAndOrganizationDTO userOrganization, int typeId = 0);
        IEnumerable<EventListItemDTO> GetEventsByTypeAndOffice(UserAndOrganizationDTO userOrganization, int? typeId = null, int? officeId = null);
    }
}
