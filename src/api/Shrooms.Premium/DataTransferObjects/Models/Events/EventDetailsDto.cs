using Shrooms.Premium.Constants;
using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ImageName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime RegistrationDeadlineDate { get; set; }

        public bool AllowMaybeGoing { get; set; }
        public bool AllowNotGoing { get; set; }

        public EventOfficesDto Offices { get; set; }
        public bool IsPinned { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int MaxParticipants { get; set; }
        public int MaxVirtualParticipants { get; set; }
        public int MaxOptions { get; set; }
        public string HostUserFullName { get; set; }
        public string HostUserId { get; set; }
        public bool IsFull { get; set; }
        public AttendingStatus ParticipatingStatus { get; set; }
        public int WallId { get; set; }
        public IEnumerable<EventDetailsOptionDto> Options { get; set; }
        public IEnumerable<EventDetailsParticipantDto> Participants { get; set; }

        public int GoingCount { get; set; }
        public int VirtuallyGoingCount { get; set; }
        public int MaybeGoingCount { get; set; }
        public int NotGoingCount { get; set; }
    }
}
