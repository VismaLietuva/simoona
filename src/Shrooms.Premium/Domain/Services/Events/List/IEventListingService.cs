using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.Services.Args;

namespace Shrooms.Premium.Domain.Services.Events.List
{
    public interface IEventListingService
    {
        Task<IEnumerable<EventListItemDTO>> GetMyEventsAsync(MyEventsOptionsDTO options, int page, int? officeId = null);
        Task<EventOptionsDTO> GetEventOptionsAsync(Guid eventId, UserAndOrganizationDTO userOrg);
        Task<IEnumerable<EventListItemDTO>> GetEventsFilteredAsync(EventsListingFilterArgs args, UserAndOrganizationDTO userOrganization);
    }
}
