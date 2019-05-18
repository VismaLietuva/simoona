using System;
using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.List
{
    public interface IEventListingService
    {
        IEnumerable<EventListItemDTO> GetMyEvents(MyEventsOptionsDTO options, int? officeId = null);
        EventOptionsDTO GetEventOptions(Guid eventId, UserAndOrganizationDTO userOrg);
        IEnumerable<EventListItemDTO> GetEventsByType(UserAndOrganizationDTO userOrganization, int typeId = 0);
        IEnumerable<EventListItemDTO> GetEventsByTypeAndOffice(UserAndOrganizationDTO userOrganization, int? typeId = null, int? officeId = null);
    }
}
