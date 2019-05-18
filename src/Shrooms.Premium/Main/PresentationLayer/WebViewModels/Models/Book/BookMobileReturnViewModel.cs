using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Book
{
    public class BookMobileReturnViewModel
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public int OrganizationId { get; set; }

        [Required]
        public int OfficeId { get; set; }
    }
}
