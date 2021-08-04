using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Books.BookDetails
{
    public class BookDetailsAdministrationDto
    {
        public BookDetailsDto BookDetails { get; set; }
        public IEnumerable<BookQuantityByOfficeDto> QuantityByOffice { get; set; }
    }
}
