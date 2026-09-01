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

        /// <summary>Ignored unless the new status is Going.</summary>
        public IEnumerable<int> ChosenOptions { get; set; }
    }
}
