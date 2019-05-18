using System;
using System.Threading.Tasks;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Events
{
    public interface IEventService
    {
        void UpdateEvent(EditEventDTO eventDto);
        Task<CreateEventDto> CreateEvent(CreateEventDto newEventDto);
        void Delete(Guid id, UserAndOrganizationDTO userOrg);
        EventDetailsDTO GetEventDetails(Guid id, UserAndOrganizationDTO userOrg);
        EventEditDTO GetEventForEditing(Guid id, UserAndOrganizationDTO userOrg);
        void CheckIfEventExists(string eventId, int organizationId);
    }
}
