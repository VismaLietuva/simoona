using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.Models.Events
{
    public class UpdateEventTypeViewModel : CreateEventTypeViewModel
    {
        [Required]
        public int Id { get; set; }
    }
}
