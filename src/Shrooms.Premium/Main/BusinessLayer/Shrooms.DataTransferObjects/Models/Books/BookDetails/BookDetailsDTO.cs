using System.Collections.Generic;

namespace Shrooms.DataTransferObjects.Models.Books.BookDetails
{
    public class BookDetailsDTO
    {
        public int BookOfficeId { get; set; }
        public int Id { get; set; }
        public string Isbn { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Url { get; set; }
        public IEnumerable<BookDetailsLogDTO> BookLogs { get; set; }
        public bool CanBeTaken { get; set; }
        public string OwnerId { get; set; }
        public string OwnerFullName { get; set; }
        public string Note { get; set; }
        public string CoverUrl { get; set; }
    }
}
