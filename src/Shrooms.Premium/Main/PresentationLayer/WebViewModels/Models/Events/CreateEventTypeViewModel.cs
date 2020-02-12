using System.ComponentModel.DataAnnotations;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Events
{
    public class CreateEventTypeViewModel
    {
        public bool IsSingleJoin { get; set; }

        public bool SendWeeklyReminders { get; set; }

        [Required]
        [StringLength(WebApiConstants.EventTypeNameMaxLength)]
        public string Name { get; set; }

        public string SingleJoinGroupName { get; set; }

        public bool IsShownWithMainEvents { get; set; }
    }
}
