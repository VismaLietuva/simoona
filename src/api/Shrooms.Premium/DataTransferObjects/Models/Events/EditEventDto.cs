using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events.Reminders;
using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EditEventDto : UserAndOrganizationDto, IEventArgsDto
    {
        public string Id { get; set; }

        public bool ResetParticipantList { get; set; }

        public bool ResetVirtualParticipantList { get; set; }

        public IEnumerable<EventOptionDto> EditedOptions { get; set; }

        public string Name { get; set; }

        public string ImageName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime RegistrationDeadlineDate { get; set; }

        public EventRecurrenceOptions Recurrence { get; set; }

        public bool AllowMaybeGoing { get; set; }

        public bool AllowNotGoing { get; set; }

        public bool IsShownInUpcomingEventsWidget { get; set; }

        public EventOfficesDto Offices { get; set; }

        public bool IsPinned { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }

        public int MaxParticipants { get; set; }

        public int MaxVirtualParticipants { get; set; }

        public int MaxOptions { get; set; }

        public int TypeId { get; set; }

        public string ResponsibleUserId { get; set; }

        public IEnumerable<NewEventOptionDto> NewOptions { get; set; }

        public IEnumerable<EventReminderDto> Reminders { get; set; }
    }
}
