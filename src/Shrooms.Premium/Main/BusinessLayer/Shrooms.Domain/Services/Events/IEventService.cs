using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Events;
using System;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Events
{
    public interface IEventService
    {
        void UpdateEvent(EditEventDTO eventDto);
        Task<CreateEventDto> CreateEvent(CreateEventDto newEventDto);
        void Delete(Guid id, UserAndOrganizationDTO userOrg);
        void ToggleEventPin(Guid id);
        EventDetailsDTO GetEventDetails(Guid id, UserAndOrganizationDTO userOrg);
        EventEditDTO GetEventForEditing(Guid id, UserAndOrganizationDTO userOrg);
        void CheckIfEventExists(string eventId, int organizationId);
    }
}
