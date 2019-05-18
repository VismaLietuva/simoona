﻿using System;
using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Events.Utilities
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

        IEnumerable<EventOptionCountDTO> GetEventChosenOptions(Guid eventId, UserAndOrganizationDTO userAndOrg);
    }
}
