using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events.Reminders;
using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventEditDetailsDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ImageName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime? RegistrationDeadlineDate { get; set; }

        public EventRecurrenceOptions Recurrence { get; set; }

        public bool AllowMaybeGoing { get; set; }

        public bool AllowNotGoing { get; set; }

        public EventOfficesDto Offices { get; set; }

        public bool IsPinned { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }

        public int TypeId { get; set; }

        public int MaxParticipants { get; set; }

        public int MaxVirtualParticipants { get; set; }

        public int MaxOptions { get; set; }

        public string HostUserId { get; set; }

        public string HostUserFullName { get; set; }

        public bool ResetParticipantList { get; set; }

        public bool IsShownInUpcomingEventsWidget { get; set; }

        public IEnumerable<EventOptionDto> Options { get; set; }

        /// <summary>
        /// The sign-up question tree, so the builder can rehydrate on edit. Same shape the
        /// options endpoint serves. Without this an edit submits an empty tree and
        /// EventQuestionWriter.SoftDeleteAbsent wipes every existing question.
        /// </summary>
        public IEnumerable<EventQuestionStructureDto> Questions { get; set; } = new List<EventQuestionStructureDto>();

        public IEnumerable<EventReminderDetailsDto> Reminders { get; set; }
    }
}
