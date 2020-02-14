using System;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events
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
