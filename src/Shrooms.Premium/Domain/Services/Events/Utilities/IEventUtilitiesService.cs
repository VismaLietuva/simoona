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

        Task<IEnumerable<EventTypeDto>> GetEventTypesAsync(int organizationId);

        Task CreateEventTypeAsync(CreateEventTypeDto eventType);

        Task UpdateEventTypeAsync(UpdateEventTypeDto eventType);

        Task DeleteEventTypeAsync(int id, UserAndOrganizationDto userAndOrg);

        Task<EventTypeDto> GetEventTypeAsync(int organizationId, int eventTypeId);

        Task<IEnumerable<EventTypeDto>> GetEventTypesToRemindAsync(int organizationId);

        Task<IEnumerable<EventOptionCountDto>> GetEventChosenOptionsAsync(Guid eventId, UserAndOrganizationDto userAndOrg);

        Task<bool> AnyEventsThisWeekByTypeAsync(IEnumerable<int> eventTypeIds);

        Task<IEnumerable<string>> GetEventTypesSingleJoinGroupsAsync(int organizationId);
    }
}
