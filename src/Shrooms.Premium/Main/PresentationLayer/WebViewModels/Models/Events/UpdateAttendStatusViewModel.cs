using System;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Events
{
    public class UpdateAttendStatusViewModel
    {
        public Guid EventId { get; set; }
        public int AttendStatus { get; set; }
        public string AttendComment { get; set; }
    }
}
