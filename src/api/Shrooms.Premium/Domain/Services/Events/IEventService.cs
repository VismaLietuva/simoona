using System;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Events
{
    public interface IEventService
    {
        Task UpdateEventAsync(EditEventDto eventDto);
        Task<CreateEventDto> CreateEventAsync(CreateEventDto newEventDto);
        Task DeleteAsync(Guid id, UserAndOrganizationDto userOrg);
        Task ToggleEventPinAsync(Guid id);
        Task<EventDetailsDto> GetEventDetailsAsync(Guid id, UserAndOrganizationDto userOrg);
        Task<EventEditDto> GetEventForEditingAsync(Guid id, UserAndOrganizationDto userOrg);
        Task CheckIfEventExistsAsync(string eventId, int organizationId);
        Task<ExtensiveEventDetailsDto> GetExtensiveEventDetailsAsync(Guid eventId, UserAndOrganizationDto userOrg, string[] kudosTypeNames, int[] eventTypes);
    }
}
