using System;
using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Events;

namespace Shrooms.Domain.Services.Events.Utilities
{
    public interface IEventUtilitiesService
    {
        string GetEventName(Guid eventId);

        IEnumerable<object> GetRecurranceOptions();

        void DeleteByEvent(Guid eventId, string userId);

        IEnumerable<EventTypeDTO> GetEventTypes(int organizationId);

        void CreateEventType(CreateEventTypeDTO eventType);

        void UpdateEventType(UpdateEventTypeDTO eventType);

        void DeleteEventType(int id, UserAndOrganizationDTO userAndOrg);

        EventTypeDTO GetEventType(int organizationId, int eventTypeId);

        IEnumerable<EventTypeDTO> GetEventTypesToRemind(int organizationId);

        IEnumerable<EventOptionCountDTO> GetEventChosenOptions(Guid eventId, UserAndOrganizationDTO userAndOrg);

        bool AnyEventsThisWeekByType(IEnumerable<int> eventTypeIds);

        IEnumerable<string> GetEventTypesSingleJoinGroups(int organizationId);
    }
}
