using System.ComponentModel.DataAnnotations;
using Shrooms.WebViewModels.ValidationAttributes;

namespace Shrooms.WebViewModels.Models.Book.BookDetails
{
    public class EditBookViewModel : NewBookViewModel
    {
        [Required]
        [MinValue(1)]
        public int Id { get; set; }
    }
}
