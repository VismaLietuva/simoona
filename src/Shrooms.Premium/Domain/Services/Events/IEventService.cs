using System;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Events
{
    public interface IEventService
    {
        Task UpdateEventAsync(EditEventDTO eventDto);
        Task<CreateEventDto> CreateEventAsync(CreateEventDto newEventDto);
        Task DeleteAsync(Guid id, UserAndOrganizationDTO userOrg);
        Task ToggleEventPinAsync(Guid id);
        Task<EventDetailsDTO> GetEventDetailsAsync(Guid id, UserAndOrganizationDTO userOrg);
        Task<EventEditDTO> GetEventForEditingAsync(Guid id, UserAndOrganizationDTO userOrg);
        Task CheckIfEventExistsAsync(string eventId, int organizationId);
    }
}
