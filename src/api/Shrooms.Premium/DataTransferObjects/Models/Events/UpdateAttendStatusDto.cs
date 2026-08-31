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
        /// Answers to the event's sign-up questions, plus any legacy option picks, for a change
        /// into Going. Validated and saved in the same call so the status cannot be reached
        /// while required questions are unanswered. Ignored for the other statuses.
        /// </summary>
        public IEnumerable<int> ChosenOptions { get; set; }
    }
}
