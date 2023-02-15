using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events.Reminders;
using System.Collections.Generic;
using System;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public interface IEventArgsDto
    {
        string Id { get; set; }

        string Name { get; set; }

        string ImageName { get; set; }

        DateTime StartDate { get; set; }

        DateTime EndDate { get; set; }

        DateTime RegistrationDeadlineDate { get; set; }

        EventRecurrenceOptions Recurrence { get; set; }

        bool AllowMaybeGoing { get; set; }

        bool AllowNotGoing { get; set; }

        bool IsShownInUpcomingEventsWidget { get; set; }

        EventOfficesDto Offices { get; set; }

        bool IsPinned { get; set; }

        string Location { get; set; }

        string Description { get; set; }

        int MaxParticipants { get; set; }

        int MaxVirtualParticipants { get; set; }

        int MaxOptions { get; set; }

        int TypeId { get; set; }

        string ResponsibleUserId { get; set; }

        IEnumerable<NewEventOptionDto> NewOptions { get; set; }

        IEnumerable<EventReminderDto> Reminders { get; set; }

        int OrganizationId { get; set; }

        string UserId { get; set; }
    }
}
