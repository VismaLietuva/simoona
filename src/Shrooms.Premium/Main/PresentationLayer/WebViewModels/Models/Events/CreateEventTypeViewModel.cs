using System.ComponentModel.DataAnnotations;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Events
{
    public class CreateEventTypeViewModel
    {
        public bool IsSingleJoin { get; set; }

        [Required]
        [StringLength(WebApiConstants.EventTypeNameMaxLength)]
        public string Name { get; set; }
    }
}
