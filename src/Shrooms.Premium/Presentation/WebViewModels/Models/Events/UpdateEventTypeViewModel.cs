using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Models.Events
{
    public class UpdateEventTypeViewModel : CreateEventTypeViewModel
    {
        [Required]
        public int Id { get; set; }
    }
}
