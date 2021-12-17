using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Book.BookDetails
{
    public class BookDetailsAdministrationViewModel
    {
        public BookDetailsViewModel BookDetails { get; set; }

        public IEnumerable<BookQuantityByOfficeViewModel> QuantityByOffice { get; set; }
    }
}
