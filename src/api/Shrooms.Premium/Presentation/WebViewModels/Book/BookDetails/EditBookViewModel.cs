using System.ComponentModel.DataAnnotations;
using Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes;

namespace Shrooms.Premium.Presentation.WebViewModels.Book.BookDetails
{
    public class EditBookViewModel : NewBookViewModel
    {
        [Required]
        [MinValue(1)]
        public int Id { get; set; }
    }
}
