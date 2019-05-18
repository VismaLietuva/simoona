using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Book
{
    public class BookMobileTakeSpecificViewModel
    {
        [Required]
        public int BookLogId { get; set; }

        [Required]
        public string UserId { get; set; }
    }
}
