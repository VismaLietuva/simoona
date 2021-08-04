using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Events.Utilities
{
    public interface IEventUtilitiesService
    {
        Task<string> GetEventNameAsync(Guid eventId);

        IEnumerable<object> GetRecurrenceOptions();

        Task DeleteByEventAsync(Guid eventId, string userId);

        Task<IEnumerable<EventTypeDTO>> GetEventTypesAsync(int organizationId);

        Task CreateEventTypeAsync(CreateEventTypeDTO eventType);

        Task UpdateEventTypeAsync(UpdateEventTypeDTO eventType);

        Task DeleteEventTypeAsync(int id, UserAndOrganizationDTO userAndOrg);

        Task<EventTypeDTO> GetEventTypeAsync(int organizationId, int eventTypeId);

        Task<IEnumerable<EventTypeDTO>> GetEventTypesToRemindAsync(int organizationId);

        Task<IEnumerable<EventOptionCountDTO>> GetEventChosenOptionsAsync(Guid eventId, UserAndOrganizationDTO userAndOrg);

        Task<bool> AnyEventsThisWeekByTypeAsync(IEnumerable<int> eventTypeIds);

        Task<IEnumerable<string>> GetEventTypesSingleJoinGroupsAsync(int organizationId);
    }
}
