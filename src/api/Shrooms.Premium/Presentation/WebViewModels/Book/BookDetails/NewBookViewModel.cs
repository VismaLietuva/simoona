using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Book.BookDetails
{
    public class NewBookViewModel
    {
        public string Isbn { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        public string Url { get; set; }

        [Required]
        public IEnumerable<NewBookQuantityViewModel> QuantityByOffice { get; set; }

        public string OwnerId { get; set; }

        public string OwnerFullName { get; set; }

        public string Note { get; set; }
    }
}
