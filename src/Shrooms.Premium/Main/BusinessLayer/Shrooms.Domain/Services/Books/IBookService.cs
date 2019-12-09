using System.Threading.Tasks;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Books;
using Shrooms.DataTransferObjects.Models.Books.BookDetails;
using Shrooms.DataTransferObjects.Models.Books.BooksByOffice;
using Shrooms.DataTransferObjects.Models.LazyPaged;

namespace Shrooms.Domain.Services.Books
{
    public interface IBookService
    {
        void AddBook(NewBookDTO bookDto);
        void TakeBook(BookTakeDTO bookDTO);
        void EditBook(EditBookDTO editedBook);
        void DeleteBook(int bookOfficeId, UserAndOrganizationDTO userOrg);
        void TakeBook(int bookOfficeId, UserAndOrganizationDTO userAndOrg);
        void ReturnBook(int bookOfficeId, UserAndOrganizationDTO userAndOrg);
        void ReportBook(BookReportDTO bookReport, UserAndOrganizationDTO userAndOrg);
        void UpdateBookCovers();
        Task<RetrievedBookInfoDTO> FindBookByIsbn(string isbn, int organizationId);
        ILazyPaged<BooksByOfficeDTO> GetBooksByOffice(BooksByOfficeOptionsDTO options);
        BookDetailsDTO GetBookDetails(int bookOfficeId, UserAndOrganizationDTO userOrg);
        BookDetailsAdministrationDTO GetBookDetailsWithOffices(int bookOfficeId, UserAndOrganizationDTO userOrg);
    }
}
