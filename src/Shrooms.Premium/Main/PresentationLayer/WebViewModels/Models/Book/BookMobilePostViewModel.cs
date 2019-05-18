using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Book
{
    public class BookMobilePostViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        public string Url { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        public int OfficeId { get; set; }

        public string ApplicationUserId { get; set; }

        [Required]
        public int OrganizationId { get; set; }
    }
}
