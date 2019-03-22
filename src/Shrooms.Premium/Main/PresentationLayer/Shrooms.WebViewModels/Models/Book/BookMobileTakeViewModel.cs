using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.Models.Book
{
    public class BookMobileTakeViewModel
    {
        [Required]
        public int BookOfficeId { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [Required]
        public int OrganizationId { get; set; }
    }
}
