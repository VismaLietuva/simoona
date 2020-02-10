using Shrooms.Constants.WebApi;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.Models.Events
{
    public class CreateEventTypeViewModel
    {
        public bool IsSingleJoin { get; set; }

        public bool SendWeeklyReminders { get; set; }

        [Required]
        [StringLength(ConstWebApi.EventTypeNameMaxLength)]
        public string Name { get; set; }

        public string SingleJoinGroupName { get; set; }

        public bool IsShownWithMainEvents { get; set; }
    }
}
