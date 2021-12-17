using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class UpdateEventTypeViewModel : CreateEventTypeViewModel
    {
        [Required]
        public int Id { get; set; }
    }
}
