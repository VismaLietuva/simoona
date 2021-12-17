using System;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventJoinDto : UserAndOrganizationDto
    {
        public Guid EventId { get; set; }

        public int AttendStatus { get; set; }

        public string AttendComment { get; set; }

        public IEnumerable<int> ChosenOptions { get; set; }

        public ICollection<string> ParticipantIds { get; set; }
    }
}
