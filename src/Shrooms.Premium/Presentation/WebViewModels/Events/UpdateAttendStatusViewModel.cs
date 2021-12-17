using System;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class UpdateAttendStatusViewModel
    {
        public Guid EventId { get; set; }
        public int AttendStatus { get; set; }
        public string AttendComment { get; set; }
    }
}
