using System;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventJoinDto : UserAndOrganizationDto
    {
        public Guid EventId { get; set; }

        public AttendingStatus AttendStatus { get; set; }

        public string AttendComment { get; set; }

        public IEnumerable<int> ChosenOptions { get; set; }

        /// <summary>
        /// Question-owned option ids. Join replaces the participant's whole selection, so null here
        /// reads as "no answers supplied" and the answer rules are enforced against that.
        /// </summary>
        public IEnumerable<int> Answers { get; set; }

        public ICollection<string> ParticipantIds { get; set; }
    }
}
