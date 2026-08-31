using Shrooms.Premium.Constants;
using System;
using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class UpdateAttendStatusViewModel
    {
        public Guid EventId { get; set; }
        public AttendingStatus AttendStatus { get; set; }
        public string AttendComment { get; set; }

        /// <summary>
        /// Answers to the sign-up questions for a change into Going, in the same shape as
        /// EventJoinViewModel.ChosenOptions. Ignored for the other statuses.
        /// </summary>
        public IEnumerable<int> ChosenOptions { get; set; }
    }
}
