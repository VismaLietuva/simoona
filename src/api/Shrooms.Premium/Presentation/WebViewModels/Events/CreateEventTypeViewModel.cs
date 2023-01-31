using System.ComponentModel.DataAnnotations;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class CreateEventTypeViewModel
    {
        public bool IsSingleJoin { get; set; }

        public bool SendWeeklyReminders { get; set; }

        [Required]
        [StringLength(EventsConstants.EventTypeNameMaxLength)]
        public string Name { get; set; }

        public string SingleJoinGroupName { get; set; }

        public bool IsShownWithMainEvents { get; set; }

        public bool SendEmailToManager { get; set; }

        public bool IsShownInUpcomingEvents { get; set; }
    }
}
