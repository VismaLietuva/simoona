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
        /// Legacy flat options. Ignored unless the new status is Going. Omit the property to keep
        /// the stored picks; send an array to replace them.
        /// </summary>
        public IEnumerable<int> ChosenOptions { get; set; }

        /// <summary>
        /// Question answers. Ignored unless the new status is Going. Omit the property to keep the
        /// stored answers — which is what the shipped client does — or send an empty array to clear
        /// them. The answer rules are only enforced when this is present.
        /// </summary>
        public IEnumerable<int> Answers { get; set; }
    }
}
