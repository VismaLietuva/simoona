using System.ComponentModel.DataAnnotations;
using Shrooms.Presentation.WebViewModels.ValidationAttributes;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Book.BookDetails
{
    public class EditBookViewModel : NewBookViewModel
    {
        [Required]
        [MinValue(1)]
        public int Id { get; set; }
    }
}
