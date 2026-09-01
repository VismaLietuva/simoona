using System;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class UpdateAttendStatusDto : UserAndOrganizationDto
    {
        public Guid EventId { get; set; }

        public AttendingStatus AttendStatus { get; set; }

        public string AttendComment { get; set; }

        /// <summary>
        /// Legacy flat options, or null to keep the participant's stored picks.
        /// </summary>
        public IEnumerable<int> ChosenOptions { get; set; }

        /// <summary>
        /// Question-owned option ids, or null to keep the stored answers and skip the answer rules.
        /// Both collections are assigned by the controller: AutoMapper turns a null collection into
        /// an empty one, and "omitted" has to stay distinguishable from "clear these".
        /// </summary>
        public IEnumerable<int> Answers { get; set; }
    }
}
