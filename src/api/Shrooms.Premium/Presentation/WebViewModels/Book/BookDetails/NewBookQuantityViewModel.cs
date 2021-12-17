using System.ComponentModel.DataAnnotations;
using Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes;

namespace Shrooms.Premium.Presentation.WebViewModels.Book.BookDetails
{
    public class NewBookQuantityViewModel
    {
        [Required]
        [MinValue(1)]
        public int OfficeId { get; set; }

        [Required]
        public int BookQuantity { get; set; }
    }
}
