using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Events
{
    public class UpdateEventTypeViewModel : CreateEventTypeViewModel
    {
        [Required]
        public int Id { get; set; }
    }
}
