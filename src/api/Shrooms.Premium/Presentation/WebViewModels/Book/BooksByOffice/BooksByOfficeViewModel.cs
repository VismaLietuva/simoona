using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Book.BooksByOffice
{
    public class BooksByOfficeViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Url { get; set; }

        public int QuantityLeft { get; set; }

        public IEnumerable<BasicBookUserViewModel> Readers { get; set; }

        public bool TakenByCurrentUser { get; set; }
    }
}
