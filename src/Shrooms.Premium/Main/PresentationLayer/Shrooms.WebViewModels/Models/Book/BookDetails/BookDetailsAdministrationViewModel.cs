using System.Collections.Generic;

namespace Shrooms.WebViewModels.Models.Book.BookDetails
{
    public class BookDetailsAdministrationViewModel
    {
        public BookDetailsViewModel BookDetails { get; set; }

        public IEnumerable<BookQuantityByOfficeViewModel> QuantityByOffice { get; set; }
    }
}
