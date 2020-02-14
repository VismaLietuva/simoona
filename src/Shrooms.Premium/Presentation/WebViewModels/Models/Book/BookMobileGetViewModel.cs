using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Models.Book
{
    public class BookMobileGetViewModel
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public int OrganizationId { get; set; }

        [Required]
        public int OfficeId { get; set; }
    }
}
