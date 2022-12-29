using Shrooms.Premium.Constants;
using System;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class UpdateAttendStatusViewModel
    {
        public Guid EventId { get; set; }
        public AttendingStatus AttendStatus { get; set; }
        public string AttendComment { get; set; }
    }
}
