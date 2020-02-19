using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class ShareEventViewModel
    {
        [Required]

        public string MessageBody { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int WallId { get; set; }

        [Required]
        public string EventsId { get; set; }
    }
}
