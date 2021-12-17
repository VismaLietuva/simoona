using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Books.BooksByOffice
{
    public class BooksByOfficeDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Url { get; set; }

        public string OwnerId { get; set; }

        public string Note { get; set; }

        public int QuantityLeft { get; set; }

        public IEnumerable<BasicBookUserDto> Readers { get; set; }

        public bool TakenByCurrentUser { get; set; }
    }
}
