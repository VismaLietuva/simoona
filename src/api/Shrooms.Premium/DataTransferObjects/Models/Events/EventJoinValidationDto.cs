using System;
using System.Collections.Generic;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventJoinValidationDto
    {
        public Guid Id { get; set; }
        public bool AlreadyParticipates { get; set; }
        public DateTime EndDate { get; set; }
        public int EventTypeId { get; set; }
        public string SingleJoinGroupName { get; set; }
        public bool IsSingleJoin { get; set; }
        public bool SendEmailToManager { get; set; }
        public int MaxChoices { get; set; }
        public int MaxParticipants { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public bool AllowMaybeGoing { get; set; }
        public bool AllowNotGoing { get; set; }

        public string Location { get; set; }
        public ICollection<EventOption> Options { get; set; }
        public ICollection<EventOption> SelectedOptions { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public DateTime StartDate { get; set; }
        public List<string> Participants { get; set; }
        public string ResponsibleUserId { get; set; }
        public int WallId { get; set; }
    }
}
