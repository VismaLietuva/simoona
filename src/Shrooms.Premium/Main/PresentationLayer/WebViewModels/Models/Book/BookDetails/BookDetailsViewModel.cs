using System.Collections.Generic;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Book.BookDetails
{
    public class BookDetailsViewModel
    {
        public int Id { get; set; }

        public int BookOfficeId { get; set; }

        public string Isbn { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Url { get; set; }

        public bool CanBeTaken { get; set; }

        public IEnumerable<BookDetailsLogViewModel> BookLogs { get; set; }

        public string OwnerId { get; set; }

        public string OwnerFullName { get; set; }

        public string Note { get; set; }

        public string CoverUrl { get; set; }
    }
}
