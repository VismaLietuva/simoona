using System.Collections.Generic;

namespace Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Books.BookDetails
{
    public class BookDetailsAdministrationDTO
    {
        public BookDetailsDTO BookDetails { get; set; }
        public IEnumerable<BookQuantityByOfficeDTO> QuantityByOffice { get; set; }
    }
}
