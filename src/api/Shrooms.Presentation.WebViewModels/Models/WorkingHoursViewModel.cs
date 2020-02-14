using System;

namespace Shrooms.Presentation.WebViewModels.Models
{
    public class WorkingHoursViewModel
    {
        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public TimeSpan? LunchStart { get; set; }

        public TimeSpan? LunchEnd { get; set; }

        public bool? FullTime { get; set; }

        public int? PartTimeHours { get; set; }
    }
}